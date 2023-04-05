# FinTrak: Personal Finance Tracking and Analysis

FinTrak is a web-based application designed to help users manage their personal finances, track spending habits, and gain insights through AI-generated recommendations. 
With an intuitive dashboard and user-friendly interface, FinTrak makes managing personal finances a breeze.

## Features
- Account Summary: Get an overview of your financial situation with a summary of your total income, total expenses, and current balance.
- Transaction Management: Add, edit, and delete transactions to keep track of your income and expenses.
- Financial Goals: Set financial goals with target amounts, start and end dates, and track your progress towards achieving them.
- Spending Analysis: Visualize your spending habits by category with an interactive pie chart.
- AI-Generated Recommendations: Receive personalized recommendations based on your spending habits, powered by OpenAI's GPT-4 model. (Note: This feature requires an active OpenAI API key with available credit.)
- Reports: Generate financial reports to gain insights into your financial health and progress towards your goals.

## Getting Started
These instructions will help you get a copy of the project up and running on your local machine for development and testing purposes.

## Installation
1- Clone the FinTrak repository to your local machine:

````bash
git clone https://github.com/yourusername/FinTrak.git
````
2- Open the FinTrak.sln file in Visual Studio or your preferred IDE.

3- Restore NuGet packages and build the solution.

4- Update the appsettings.json file with your OpenAI API key and any other necessary configuration settings.

5- Run the application using the development web server (e.g., IIS Express in Visual Studio).

## Deployment
Refer to the [Azure deployment guide](https://azure.microsoft.com/en-us/resources/whitepapers/developer-guide-to-azure/) for instructions on how to deploy the application to Azure App Service. You can also set up continuous integration and continuous deployment (CI/CD) using [GitHub Actions](https://docs.github.com/en/actions) or [Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/?view=azure-devops).

## Contributing
If you'd like to contribute to FinTrak, feel free to fork the repository, create a feature branch, and submit a pull request. Please ensure that your code adheres to the existing style and structure.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments
- OpenAI's GPT-4 model for providing AI-generated recommendations.
- Chart.js for creating interactive spending analysis charts.



