#!/bin/bash

# Manual Polly Test - For WebUI running in Rider
# This script requires you to STOP Rider WebUI before running

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

API_URL="https://localhost:7144/api/productapi"

print_success() { echo -e "${GREEN}✅ $1${NC}"; }
print_error() { echo -e "${RED}❌ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
print_info() { echo -e "${BLUE}ℹ️  $1${NC}"; }

wait_for_service() {
    local service=$1
    local port=$2
    local max_attempts=30
    local attempt=0
    
    print_info "Waiting for $service..."
    
    while [ $attempt -lt $max_attempts ]; do
        if nc -z localhost $port 2>/dev/null; then
            print_success "$service is ready!"
            return 0
        fi
        attempt=$((attempt + 1))
        echo -n "."
        sleep 1
    done
    
    print_error "$service failed to start"
    return 1
}

clear
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}🔬 POLLY MANUAL TEST${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
print_warning "⚠️  IMPORTANT: You must STOP WebUI in Rider before proceeding!"
echo ""
echo "Steps to follow:"
echo "  1. In Rider: Stop the WebUI (Shift+F5 or Stop button)"
echo "  2. Keep PostgreSQL, Redis, RabbitMQ running"
echo "  3. Press Enter when ready to start test"
echo ""
read -p "Press Enter when WebUI is STOPPED..."

# Check if WebUI is really stopped
if nc -z localhost 5000 2>/dev/null; then
    print_error "WebUI is still running on port 5000!"
    print_error "Please STOP it in Rider first!"
    exit 1
fi

print_success "WebUI confirmed stopped"
echo ""

# ============================================
# PART 1: Database Retry Test
# ============================================
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}TEST 1: Database Retry (14s expected)${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

print_info "Clearing Redis cache..."
docker exec product-redis redis-cli FLUSHALL > /dev/null 2>&1
sleep 1

print_info "Stopping PostgreSQL..."
docker stop product-postgres > /dev/null 2>&1
sleep 2

print_info ""
print_info "NOW: Start WebUI in Rider (F5 or Run)"
print_info "Wait for 'Now listening on' message"
echo ""
read -p "Press Enter when WebUI is running..."

if ! nc -z localhost 5000 2>/dev/null; then
    print_error "WebUI is not running on port 5000!"
    exit 1
fi

print_success "WebUI detected on port 5000"
sleep 2

print_info ""
print_info "Making first request to API..."
print_info "Expected: 500/503 after ~14s (3 retries with exponential backoff)"
print_info "Watching for: 2s pause → 4s pause → 8s pause"
print_info "---------------------------------------------------"

start=$(date +%s)

# Make request
status=$(curl -k -s -o /tmp/response.txt -w "%{http_code}" -m 20 "$API_URL" 2>/dev/null || echo "timeout")

end=$(date +%s)
duration=$((end - start))

response_preview=$(cat /tmp/response.txt 2>/dev/null | head -c 150)

print_info "---------------------------------------------------"
echo ""
print_info "RESULTS:"
echo "  Status Code: $status"
echo "  Duration: ${duration}s"
echo "  Response: ${response_preview:0:100}"
echo ""

# Analyze
if [ "$status" = "500" ] || [ "$status" = "503" ]; then
    if [ $duration -ge 12 ] && [ $duration -le 16 ]; then
        print_success "✅ PERFECT! Polly retry working correctly!"
        print_info "Duration ${duration}s matches expected ~14s (2+4+8 seconds)"
    elif [ $duration -ge 8 ]; then
        print_warning "⚠️  Duration ${duration}s (expected 14s)"
        print_info "Polly is retrying but timing differs slightly"
    elif [ $duration -ge 4 ]; then
        print_warning "⚠️  Only ${duration}s - fewer retries than expected"
        print_info "Check Polly config: may be only 2 retries instead of 3"
    else
        print_error "❌ Failed too quickly (${duration}s)"
        print_info "Polly retry may not be configured properly"
    fi
elif [ "$status" = "200" ]; then
    print_error "❌ Got 200 OK - This should NOT happen!"
    print_info "Database is down but API returned success"
    print_info "Possible issues:"
    echo "  - Response cached in memory"
    echo "  - Connection pool still has live connections"
    echo "  - Polly not applied to repository"
else
    print_warning "Unexpected status: $status after ${duration}s"
fi

print_info ""
print_info "Starting PostgreSQL..."
docker start product-postgres > /dev/null 2>&1
wait_for_service "PostgreSQL" 5432
sleep 3

print_info "Testing recovery..."
docker exec product-redis redis-cli FLUSHALL > /dev/null 2>&1
sleep 1

status=$(curl -k -s -o /dev/null -w "%{http_code}" "$API_URL" 2>/dev/null || echo "000")

if [ "$status" = "200" ]; then
    print_success "✅ Service recovered successfully!"
else
    print_error "❌ Still failing: $status"
fi

# ============================================
# PART 2: RabbitMQ Retry Test
# ============================================
echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}TEST 2: RabbitMQ Retry${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

print_info "Stopping RabbitMQ..."
docker stop product-rabbitmq > /dev/null 2>&1
sleep 2

print_info "Creating product (DB works, RabbitMQ should retry)..."
test_json='{"name":"Polly MQ Test","description":"Testing","price":99.99,"stock":10}'

response=$(curl -k -s -X POST "$API_URL" \
    -H "Content-Type: application/json" \
    -d "$test_json")

product_id=$(echo "$response" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

if [ -n "$product_id" ]; then
    print_success "✅ Product created despite RabbitMQ down!"
    print_info "Product ID: $product_id"
    print_info "Check WebUI console for RabbitMQ retry warnings"
else
    print_error "❌ Product creation failed"
    print_info "Response: ${response:0:200}"
fi

print_info ""
print_info "Starting RabbitMQ..."
docker start product-rabbitmq > /dev/null 2>&1
wait_for_service "RabbitMQ" 5672

print_success "RabbitMQ test completed!"

# ============================================
# PART 3: Redis Fallback Test
# ============================================
echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}TEST 3: Redis Fallback${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

print_info "Stopping Redis..."
docker stop product-redis > /dev/null 2>&1
sleep 2

print_info "Making request (should fallback to DB)..."
start=$(date +%s)

status=$(curl -k -s -o /dev/null -w "%{http_code}" "$API_URL" 2>/dev/null || echo "000")

end=$(date +%s)
duration=$((end - start))

if [ "$status" = "200" ]; then
    print_success "✅ App works without Redis (fallback to DB)!"
    print_info "Fallback time: ${duration}s"
else
    print_error "❌ Failed without Redis: $status"
fi

print_info "Starting Redis..."
docker start product-redis > /dev/null 2>&1
wait_for_service "Redis" 6379

# ============================================
# SUMMARY
# ============================================
echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}📊 MANUAL TEST SUMMARY${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

print_info "Review the results above and check:"
echo "  1. Database retry duration (should be ~14s)"
echo "  2. RabbitMQ product creation success"
echo "  3. Redis fallback working"
echo ""
print_info "Check Rider console for Polly retry logs"
print_info "Expected logs:"
echo "  - 'Polly Retry: Attempt 1 of 3'"
echo "  - 'Polly Retry: Attempt 2 of 3'"
echo "  - 'Polly Retry: Attempt 3 of 3'"
echo ""

print_success "Manual test completed!"
echo ""

rm -f /tmp/response.txt