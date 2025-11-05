.PHONY: help install run test coverage clean demo

# Default target - show help
help:
	@echo "CarDex CLI - Available Commands"
	@echo "================================"
	@echo ""
	@echo "  make install   - Install Python dependencies"
	@echo "  make run       - Run the CLI application"
	@echo "  make test      - Run all tests"
	@echo "  make coverage  - Run tests with coverage report"
	@echo "  make demo      - Run CLI with demo commands"
	@echo "  make clean     - Remove generated files"
	@echo ""

# Install dependencies
install:
	@echo "Installing dependencies..."
	pip install -r requirements.txt --break-system-packages
	@echo "✅ Installation complete!"

# Run the CLI
run:
	@echo "Starting CarDex CLI..."
	@echo ""
	python cardex_cli.py

# Run tests
test:
	@echo "Running tests..."
	pytest test_cardex_cli.py -v

# Run tests with coverage report
coverage:
	@echo "Running tests with coverage..."
	pytest test_cardex_cli.py -v --cov=. --cov-report=term-missing --cov-report=html
	@echo ""
	@echo "📊 Coverage report generated!"
	@echo "   View HTML report: htmlcov/index.html"

# Run CLI with demo commands
demo:
	@echo "Running CLI demo..."
	@echo ""
	@echo "help" | python cardex_cli.py
	@echo ""
	@echo "---"
	@echo ""
	@echo "vroom" | python cardex_cli.py

# Clean up generated files
clean:
	@echo "Cleaning up..."
	rm -rf __pycache__
	rm -rf .pytest_cache
	rm -rf htmlcov
	rm -rf .coverage
	@echo "✅ Cleanup complete!"