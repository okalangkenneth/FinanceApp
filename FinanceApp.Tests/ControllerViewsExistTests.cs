using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FinanceApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinanceApp.Tests
{
    // Guards against the audit-era failure mode of controller actions whose
    // views were never created (HomeController.Dashboard, Account.Profile):
    // every public action must either have its .cshtml on disk by MVC naming
    // convention or be a known non-view endpoint listed below.
    public class ControllerViewsExistTests
    {
        // Actions that never render a view: redirects after POST, file exports.
        private static readonly HashSet<string> NonViewActions = new()
        {
            "Account.Logout",
            "FinancialGoals.DeleteConfirmed",
            "Transactions.DeleteConfirmed",
            "Export.ExportTransactionsReportExcel",
            "Export.ExportTransactionsReportPdf",
        };

        private static string FindProjectRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "FinanceApp", "Views")))
            {
                dir = dir.Parent;
            }
            Assert.NotNull(dir);
            return Path.Combine(dir!.FullName, "FinanceApp", "Views");
        }

        public static IEnumerable<object[]> AllControllerActions()
        {
            var controllers = typeof(HomeController).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var controller in controllers)
            {
                var name = controller.Name.Replace("Controller", "");
                var actions = controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() == null)
                    .Where(m => typeof(IActionResult).IsAssignableFrom(Unwrap(m.ReturnType)));

                foreach (var action in actions.Select(m => m.Name).Distinct())
                {
                    yield return new object[] { name, action };
                }
            }
        }

        private static Type Unwrap(Type t) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)
                ? t.GetGenericArguments()[0]
                : t;

        [Theory]
        [MemberData(nameof(AllControllerActions))]
        public void Action_has_view_or_is_known_non_view(string controller, string action)
        {
            if (NonViewActions.Contains($"{controller}.{action}"))
            {
                return;
            }

            var viewName = action;
            var viewsRoot = FindProjectRoot();
            var candidates = new[]
            {
                Path.Combine(viewsRoot, controller, viewName + ".cshtml"),
                Path.Combine(viewsRoot, "Shared", viewName + ".cshtml"),
            };

            Assert.True(candidates.Any(File.Exists),
                $"{controller}Controller.{action} renders a view but neither " +
                $"Views/{controller}/{viewName}.cshtml nor Views/Shared/{viewName}.cshtml exists.");
        }
    }
}
