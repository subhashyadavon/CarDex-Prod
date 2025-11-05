#!/bin/bash

# Regression Test Runner Script
# This script runs the regression test suite to verify critical functionality
# Usage: ./run-regression-tests.sh

set -e

echo "========================================="
echo "Running Regression Test Suite"
echo "========================================="
echo ""

# Navigate to CarDexBackend directory
cd "$(dirname "$0")/.."

# Run regression tests
echo "Executing regression tests..."
dotnet test CarDexBackend.sln \
    --filter "Category=Regression" \
    --verbosity normal \
    --logger "trx;LogFileName=regression-tests.trx" \
    --results-directory:./TestResults/Regression

# Check exit code
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ All regression tests passed!"
    echo ""
    echo "Regression test results saved to: TestResults/Regression"
    exit 0
else
    echo ""
    echo "❌ Regression tests failed!"
    echo "Please review the output above for details."
    exit 1
fi

