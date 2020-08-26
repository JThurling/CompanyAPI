using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompanyEmployees.Controllers;
using Contracts;
using Entities.DataTransferObjects;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace CompanyEmployees.Utility
{
    public class EmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeeDto, string fields, Guid companyId,
            HttpContext context)
        {
            var shapedEmployees = ShapeData(employeeDto, fields);

            if (ShouldGenerateLinks(context))
            {
                return ReturnLinkedEmployees(employeeDto, fields, companyId, context, shapedEmployees);
            }

            return ReturnShapedEmployees(shapedEmployees);
        }

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeeDto, string fields, Guid companyId, HttpContext context,
            List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeeDto.ToList();

            for (var index = 0; index < employeeDtoList.Count; index++)
            {
                var employeeLinks = CreateLinksForEmployee(context, companyId, employeeDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(context, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid id, string fields = "")
        {
            var links = new List<Link>
            {
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new { companyId, id, fields }),
                    "self",
                    "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new { companyId, id }),
                    "delete_employee",
                    "DELETE"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new { companyId, id }),
                    "update_employee",
                    "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new { companyId, id }),
                    "partially_update_employee",
                    "PATCH")
            };

            return links;
        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper)
        {
            employeesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { }),
                "self",
                "GET"));

            return employeesWrapper;
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees) =>
            new LinkResponse { ShapedEntities = shapedEmployees };

        private bool ShouldGenerateLinks(HttpContext context)
        {
            var mediaType = (MediaTypeHeaderValue)context.Items["AcceptHeaderMediaType"];

            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeeDto, string fields) =>
            _dataShaper.ShapeData(employeeDto, fields).Select(e => e.Entity).ToList();
    }
}
