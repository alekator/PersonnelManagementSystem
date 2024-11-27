
# Personnel Management System
![image](https://github.com/user-attachments/assets/1f942007-09b2-4ecf-b174-66dfc5602865)

## Project Description
The Personnel Management System is a Windows application designed to manage a list of personnel and organizations. It allows adding, editing, and deleting organizations and employees, as well as uploading employee photos. The application supports connecting to an MS SQL database and provides an intuitive interface for managing data.

## Key Features
- **Database Integration**: Works with MS SQL Server for data storage without using ORM.
- **Asynchronous Operations**: Uses asynchronous methods for database interactions to ensure high performance.
- **CRUD Operations**: Add, edit, and delete organizations and employees with ease.
- **Real-Time Search**: Filter organization and employee lists dynamically based on user input.
- **Photo Upload**: Upload and crop employee photos to specified proportions.
- **Stress Test**: Simulate adding 10 organizations, each with 100 employees.
- **Logging**: Logs all actions to a text file for monitoring and debugging.
- **User-Friendly Interface**: Built using WPF (Windows Presentation Foundation) for a seamless user experience.

## Technologies Used
- **Programming Language**: C#
- **GUI Framework**: Windows Presentation Foundation (WPF)
- **Database**: Microsoft SQL Server
- **Logging**: Custom text file logging
- **Asynchronous Programming**: Task-based asynchronous methods for database operations

## Requirements
- Visual Studio 2019 or later
- .NET Framework 4.7.2 or later
- Microsoft SQL Server
- No additional NuGet libraries required

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repository.git
   ```
2. Open the project in Visual Studio.
3. Configure the database connection string within the application interface.
4. Build and run the application.

## Key Files
- `MainWindow.xaml` and `MainWindow.xaml.cs`: Main interface and application logic.
- `Database.cs`: Handles database interactions.
- `Logger.cs`: Implements logging functionality.
- `ConnectionSettingsWindow.xaml` and `.cs`: Interface and logic for configuring database connection settings.
- `EmployeeEditWindow.xaml` and `.cs`: Interface and logic for managing employee details.
- `OrganizationEditWindow.xaml` and `.cs`: Interface and logic for managing organization details.

## Database Setup
1. Open SQL Server Management Studio (SSMS).
2. Connect to your database server.
3. Right-click on **Databases** in the Object Explorer and select **Restore Database...**.
4. Choose **Device**, click **Add**, and select the provided `.bak` file.
5. Follow the prompts to restore the database.
6. Update the connection string in the application if necessary.

## Features in Detail
### Real-Time Search
- Implements dynamic filtering for both organizations and employees based on input in the search text fields.

### Photo Upload
- Supports uploading employee photos with cropping to a specific aspect ratio.

### Stress Test
- Automatically generates and inserts 10 organizations, each containing 100 employees, for performance testing.

### Logging
- Logs every critical action (e.g., adding/deleting/updating data, database operations, and error handling) to a file for transparency and debugging.

## License
This project is licensed under the MIT License.
