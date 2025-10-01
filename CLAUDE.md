# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

KAP_InventoryManager is a Windows desktop inventory management application built with:
- **Technology Stack**: WPF (.NET Framework 4.8), C#
- **Architecture**: MVVM (Model-View-ViewModel) pattern using MVVM Light toolkit
- **Database**: MySQL (DigitalOcean hosted)
- **UI Framework**: WPF with custom controls and styling
- **PDF Generation**: QuestPDF and iText for invoices and reports
- **Data Export**: EPPlus for Excel reports

## Build and Development Commands

This is a Visual Studio .NET Framework project that uses MSBuild:

### Building the Project
```bash
# Build the solution in Debug mode
dotnet build KAP_InventoryManager.sln -c Debug

# Build the solution in Release mode
dotnet build KAP_InventoryManager.sln -c Release

# Restore NuGet packages
nuget restore KAP_InventoryManager.sln
```

### Running the Application
```bash
# Run from Visual Studio or build output
.\KAP_InventoryManager\bin\Debug\KAP_InventoryManager.exe
```

**Note**: This project requires Visual Studio 2017+ or MSBuild tools. The target framework is .NET Framework 4.8.

## Architecture Overview

### MVVM Structure
- **Models**: Domain models and repository interfaces (`/Model/`)
- **Views**: WPF UserControls and Windows (`/View/`)
- **ViewModels**: Business logic and data binding (`/ViewModel/`)
- **Repositories**: Data access layer (`/Repositories/`)

### Key Components

#### Database Layer
- `RepositoryBase.cs`: Base class for all repositories with MySQL connection management
- Repository pattern implementation for each entity (Customer, Invoice, Item, etc.)
- Connection string configured in `App.config`

#### Business Entities
- **Customer Management**: Customer information, invoicing, and reporting
- **Inventory Management**: Items, stock tracking, transactions
- **Invoice System**: Invoice creation, payment tracking, VAT handling
- **Returns Processing**: Return items management
- **Sales Reporting**: Comprehensive reporting with multiple export formats

#### UI Architecture
- **Custom Controls**: Reusable controls in `/CustomControls/` (SearchBar, NumberCard, TextCard, etc.)
- **Styling**: Centralized styles in `/Styles/` directory
- **Modal System**: Modal views for CRUD operations
- **Navigation**: Panel-based navigation system

#### Report Generation
- **PDF Reports**: QuestPDF for invoice generation and customer reports
- **Excel Export**: EPPlus for data exports
- **Report Types**: Customer reports, sales reports, invoice summaries, representative reports

### Key Libraries and Dependencies
- **MVVM Light**: MVVM framework and commanding
- **MySql.Data**: MySQL database connectivity
- **QuestPDF**: Modern PDF generation
- **EPPlus**: Excel file generation
- **LiveCharts**: Data visualization and charting
- **FontAwesome.Sharp**: Icon library
- **Extended.Wpf.Toolkit**: Additional WPF controls

## File Organization

```
KAP_InventoryManager/
├── Model/                 # Domain models and interfaces
├── View/                  # WPF views and user controls
│   ├── Modals/           # Modal dialogs
│   └── InventoryPanelViews/ # Inventory sub-views
├── ViewModel/             # MVVM view models
│   └── ModalViewModels/  # Modal-specific view models
├── Repositories/          # Data access layer
├── Utils/                 # Report generation utilities
├── CustomControls/        # Reusable UI controls
├── Styles/               # WPF styling resources
└── Images/               # Application assets
```

## Database Configuration

The application connects to a MySQL database hosted on DigitalOcean. Connection configuration is in `App.config`:
- Production connection string is configured for the live database
- Repository base class handles connection management and disposal

## Development Notes

### Adding New Features
1. Create models in `/Model/` directory
2. Implement repository interface and concrete implementation
3. Create ViewModel with proper commanding
4. Design View with data binding
5. Add appropriate styling in `/Styles/`

### Report Development
- Use `QuestPDF` for PDF generation (Community license configured)
- Use `EPPlus` for Excel exports
- Report utilities are in `/Utils/` directory
- Follow existing patterns for date filtering and data formatting

### UI Development
- Follow MVVM pattern strictly
- Use existing custom controls when possible
- Maintain consistent styling using existing style resources
- Implement proper data validation and error handling

## Common Operations

### Database Operations
All repositories inherit from `RepositoryBase` which provides:
- Connection management
- Common database operations
- Proper resource disposal

### PDF Generation
Reports use QuestPDF with community license. Key patterns:
- Page setup with consistent margins and fonts
- Table generation with proper styling
- Header/footer management
- File organization by report type and date

### Modal Management
Modal dialogs follow a consistent pattern:
- Modal ViewModels in `/ViewModel/ModalViewModels/`
- Modal Views in `/View/Modals/`
- Proper data binding and command implementation