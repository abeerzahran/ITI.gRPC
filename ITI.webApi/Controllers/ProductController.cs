using Grpc.Core;
using Grpc.Net.Client;
using ITI.GrpcProductClient.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static  ITI.GrpcProductClient.Protos.productService;

namespace ITI.webApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AddProduct(product product)
        {
            var channel = GrpcChannel.ForAddress(" https://localhost:7097");
            var client = new productServiceClient(channel);

            var request = new product
            {
                Id = product.Id,
                Price = product.Price,
                Name = product.Name,
                Quantity = product.Quantity,
                Category = product.Category
                
            };

            state state = await client.getProductByIdAsync(new productId { Id = product.Id });
            if(state.Exist)
            {
                product Updatedproduct = await client.updateProductAsync(request);
                return  Ok(Updatedproduct);
            }
            else
            {
                product newProduct = await client.addProductAsync(request);
                return Created();
            }

            
        }

        [HttpPost]
        [Route("AddBulkProducts")]
        public async Task<ActionResult> addBulkProducts(List<product>Products) {
            var channel = GrpcChannel.ForAddress(" https://localhost:7097");
            var client = new productServiceClient(channel);
            var call = client.addBulkProduct();

             foreach (var item in Products)
            {
                await call.RequestStream.WriteAsync(new product
                {
                    Id=item.Id,
                    Price = item.Price,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Category= item.Category
                    
                });
            }
             await call.RequestStream.CompleteAsync();
            return Ok(call.ResponseAsync.Result.Num);

        }

        [HttpGet]
        public async Task<ActionResult> generateProductReport([FromQuery]Details details)
        {
            var channel = GrpcChannel.ForAddress(" https://localhost:7097");
            var client = new productServiceClient(channel);
            var call= client.generateProductReport(details);
            List<product> products = new List<product>();

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(item.Name);
                products.Add(item);
            }
            return Ok(products);
        }


    }
}
