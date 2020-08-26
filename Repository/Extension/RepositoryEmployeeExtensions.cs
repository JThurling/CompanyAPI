using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Entities.Models;
using System.Linq.Dynamic.Core;
using Repository.Extension.Utility;

namespace Repository.Extension
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> FilterEmployees(this IQueryable<Employee> employees, uint minAge,
            uint maxAge) =>
            employees.Where(e => (e.Age >= minAge && e.Age <= maxAge));

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return employees;
            }

            var lowerCaseTerm = searchTerm.Trim().ToLower();

            return employees.Where(e => e.Name.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            // Check if the string is null
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return employees.OrderBy(e => e.Name);

            // Pass through the order query string
            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Employee>(orderByQueryString);
            // Checks if the orderQuery is Null
            if (string.IsNullOrWhiteSpace(orderQuery))
                return employees.OrderBy(e => e.Name);

            return employees.OrderBy(orderQuery);
        }
    }
}
