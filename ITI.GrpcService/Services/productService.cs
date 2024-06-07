using Grpc.Core;
using ITI.GrpcProductService.Protos;
using ITI.GrpcService;
using Microsoft.AspNetCore.Http.HttpResults;
using static ITI.GrpcProductService.Protos.productService;
namespace ITI.GrpcService.Services
{
    public class productService : productServiceBase
    {
        private readonly ILogger<productService> _logger;

        public static List<product> products = new List<product>();
        public productService(ILogger<productService> logger)
        {
            _logger = logger;
        }

        public override async Task<GrpcProductService.Protos.state> getProductById(GrpcProductService.Protos.productId request, ServerCallContext context)
        {
            product? product=  products.FirstOrDefault(p => p.Id == request.Id);
            if (product == null)
            {
                return await Task.FromResult( new state { Exist = false }  );
            }
            return new state { Exist = true };

            //return base.getProductById(request, context);
        }

        public override async Task<GrpcProductService.Protos.product> addProduct(GrpcProductService.Protos.product request, ServerCallContext context)
        {
            products.Add(request);

            return await Task.FromResult(new product { Id=request.Id , Name=request.Name, Price=request.Price, Quantity=request.Quantity});
            
        }

        public override async Task<GrpcProductService.Protos.product> updateProduct(GrpcProductService.Protos.product request, ServerCallContext context)
        {
            product product = products.FirstOrDefault(p=>p.Id == request.Id);
            product.Name=request.Name;
            product.Price=request.Price;
            product.Quantity=request.Quantity;

            return await Task.FromResult(new product { Id = request.Id, Name = request.Name, Price = request.Price, Quantity = request.Quantity });

        }

        public override async Task<productsNum> addBulkProduct(IAsyncStreamReader<product> requestStream, ServerCallContext context)
        {
            var productsNum = 0;
            await foreach (var item in requestStream.ReadAllAsync())
            {
                products.Add(item);
                productsNum++;
            }
            
            return (new productsNum { Num=productsNum});
            //return base.addBulkProduct(requestStream, context);
        }

        public override async Task generateProductReport(Details request, IServerStreamWriter<product> responseStream, ServerCallContext context)
        {
            List<product> Products;
            Products = products.Where(x => x.Category == request.FilterByCategory).ToList();
            if (request.OrderByPrice)
            {
                Products = Products.OrderBy(p => p.Price).ToList();
            }
            foreach (var item in Products)
            {
                await responseStream.WriteAsync(item);
            }
            

        }


    }
}
