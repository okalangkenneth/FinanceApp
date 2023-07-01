# FinanceAI: A Smart Personal Finance Manager Leveraging .NET Core and OpenAI.

## **Introduction**

FinanceAI is a cutting-edge, user-centric web application designed to revolutionize personal finance management. Developed with .NET Core, this application harnesses the power of OpenAI to provide users with a comprehensive suite of tools for tracking income, expenses, and financial goals, offering a clear and insightful view of their financial health.

FinanceAI stands out with its unique integration of OpenAI services. This advanced technology is used to analyze spending habits and generate personalized recommendations, providing users with valuable insights and helping them make informed financial decisions. With its robust functionality and intelligent features, FinanceAI is not just a finance tracking tool - it's a smart financial advisor that fits in your pocket.

## **Features**

1. **User Authentication**: The application provides secure user authentication using ASP.NET Core's Identity Framework. Users can register, log in, and manage their accounts.
2. **Dashboard**: The dashboard provides a comprehensive overview of the user's financial status. It displays total income, expenses, and net worth. The net worth is visualized in a dynamic chart that updates in real-time using SignalR.
3. **Transaction Management**: Users can record and categorize their income and expenses. The application supports various transaction categories, including food, utilities, rent, transportation, health, entertainment, education, savings, and others.
4. **Financial Goals**: Users can set, update, and track their financial goals. Each goal includes a title, description, target amount, current amount, start date, end date, and status.
5. **Spending Analysis**: The application uses OpenAI services to analyze spending habits and generate recommendations. This feature provides valuable insights into the user's spending patterns and offers suggestions for improvement.
6. **Email Notifications**: The application uses SendGrid to send email notifications for account-related activities, such as email confirmation.
7. **Data Export**: Users can export their financial data for further analysis or backup.
8. **Currency Support**: The application supports multiple currencies, allowing users to record transactions in their preferred currency.
9. **Responsive Design**: The application uses Bootstrap for a responsive design that works well on both desktop and mobile devices.
10. **Reports**: Users can generate detailed financial reports, providing a comprehensive view of their financial status and history. The reports include a list of transactions and a monthly budget view.

## **Technical Details**

The FinanceApp is a comprehensive personal finance management web application built with the following technologies:

1. **ASP.NET Core MVC**: The application is built on the ASP.NET Core MVC framework, which is a model-view-controller framework for building dynamic web sites with clean separation of concerns.
2. **Entity Framework Core**: This is used as the Object-Relational Mapping (ORM) framework to interact with the database.
3. **SQL Server/Npgsql**: The application uses SQL Server for development and Npgsql (PostgreSQL) for production as the database.
4. **Identity Framework**: This is used for user management and authentication.
5. **SendGrid**: This is used for sending emails, such as email confirmation messages.
6. **OpenAI Services**: The application uses OpenAI services to analyze spending habits and generate recommendations. This is done in the OpenAIService class, which sends requests to the OpenAI API and processes the responses.
7. **SignalR**: This is used for real-time web functionality in the application. It enables real-time updates of the Net Worth chart on the dashboard.
8. **Chart.js**: This is used for creating charts in the application, such as the Net Worth chart on the dashboard.
9. **Bootstrap**: This is used for designing and customizing the user interface of the application.
10. **jQuery**: This is used for handling events, creating animations, and simplifying HTTP requests.

The application follows the MVC architectural pattern, with separate models, views, and controllers for different parts of the application. The models define the data structures, the views define how the data is presented to the user, and the controllers handle user input and interactions.

The application also follows good security practices, such as hashing and salting passwords, validating user input, and protecting against cross-site scripting (XSS) and cross-site request forgery (CSRF) attacks.

## ****Features****

[data:image/svg+xml,%3csvg%20xmlns=%27http://www.w3.org/2000/svg%27%20version=%271.1%27%20width=%2738%27%20height=%2738%27/%3e](data:image/svg+xml,%3csvg%20xmlns=%27http://www.w3.org/2000/svg%27%20version=%271.1%27%20width=%2738%27%20height=%2738%27/%3e)

1. **User Authentication**: The application provides secure user authentication using ASP.NET Core's Identity Framework. Users can register, log in, and manage their accounts.
2. **Dashboard**: The dashboard provides a comprehensive overview of the user's financial status. It displays total income, expenses, and net worth. The net worth is visualized in a dynamic chart that updates in real-time using SignalR.
3. **Transaction Management**: Users can record and categorize their income and expenses. The application supports various transaction categories, including food, utilities, rent, transportation, health, entertainment, education, savings, and others.
4. **Financial Goals**: Users can set, update, and track their financial goals. Each goal includes a title, description, target amount, current amount, start date, end date, and status.
5. **Spending Analysis**: The application uses OpenAI services to analyze spending habits and generate recommendations. This feature provides valuable insights into the user's spending patterns and offers suggestions for improvement.
6. **Email Notifications**: The application uses SendGrid to send email notifications for account-related activities, such as email confirmation.
7. **Data Export**: Users can export their financial data for further analysis or backup.
8. **Currency Support**: The application supports multiple currencies, allowing users to record transactions in their preferred currency.
9. **Responsive Design**: The application uses Bootstrap for a responsive design that works well on both desktop and mobile devices.
10. **Reports**: Users can generate detailed financial reports, providing a comprehensive view of their financial status and history. The reports include a list of transactions and a monthly budget view.

## **Challenges and Solutions**

One of the main challenges in developing the Personal Finance Manager was ensuring efficient data retrieval and processing, especially for the dashboard which displays a summary of the user's financial data. This was addressed by optimizing database queries and performing calculations directly in the database query itself, reducing the amount of data retrieved and the time taken to perform these calculations.

## **Conclusion**

The Personal Finance Manager is a comprehensive tool for personal finance management, offering a wide range of features to help users track and manage their finances. With its user-friendly interface and robust backend, it provides a reliable and efficient solution for personal finance management.

##
