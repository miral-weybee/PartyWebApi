using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PartyProductWebApi.Models;
using ProjectWebApi.Models;

namespace PartyProductWebApi.Controllers
{

    [Route("Invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly PartyProductWebApiContext _context;

        private readonly string _connectionString;
        public InvoiceController(PartyProductWebApiContext context, IConfiguration connectionStrings)
        {
            this._context = context;
            _connectionString = connectionStrings.GetConnectionString("defaultConnection");
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> SaveInvoice([FromBody] Invoice invoiceAddDTO)
        {
            if (invoiceAddDTO == null)
                return BadRequest();

            _context.Invoices.Add(new Invoice
            {
                Id = invoiceAddDTO.Id,
                CurrentRate = invoiceAddDTO.CurrentRate,
                Quantity = invoiceAddDTO.Quantity,
                PartyId = invoiceAddDTO.PartyId,
                ProductId = invoiceAddDTO.ProductId,
                Date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
            });
            await _context.SaveChangesAsync();
            return Ok("Invoice Added Successfully..");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateInvoice([FromRoute] int id, Invoice invoiceAddDto)
        {
            var invoice = _context.Invoices.Where(x => x.Id == id).FirstOrDefault();
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.CurrentRate = invoiceAddDto.CurrentRate;
            invoice.Quantity = invoiceAddDto.Quantity;
            invoice.PartyId = invoiceAddDto.PartyId;
            invoice.ProductId = invoiceAddDto.ProductId;
            invoice.Date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);


            await _context.SaveChangesAsync();
            return Ok("Invoice Updated Successfully..");
        }

        [HttpGet("{partyId}")]
        public async Task<ActionResult> GetInvoiceList(int partyId, [FromQuery] string productName = null, [FromQuery] string date = null)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetInvoicesByPartyAndProductAndDate", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var response = new List<InvoiceGetDTO>();
                    cmd.Parameters.Add(new SqlParameter("@PartyId", partyId));
                    cmd.Parameters.Add(new SqlParameter("@ProductName", productName));
                    cmd.Parameters.Add(new SqlParameter("@Date", date));
                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(MapToValue(reader));
                        }
                    }

                    return Ok(response);
                }
            }

        }
        private InvoiceGetDTO MapToValue(SqlDataReader reader)
        {
            return new InvoiceGetDTO()
            {
                InvoiceId = (int)reader["Id"],
                PartyName = reader["PartyName"].ToString(),
                ProductName = reader["ProductName"].ToString(),
                CurrentRate = (int)reader["CurrentRate"],
                Quantity = (int)reader["Quantity"],
                Date = reader["Date"].ToString()
            };
        }

    }
}
