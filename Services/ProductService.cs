using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IRabbitMQService _rabbitMQService;

    public ProductService(IProductRepository productRepository, IRabbitMQService rabbitMQService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
//Muito espaco utilizar ferramentas de refatoraçao para melhorar a legibilidade, nada demais tambem so uma dica

        try
        {
            return await _productRepository.GetAllAsync();


        }
        catch (Exception ex)
        {
            throw new ProductServiceException("Erro ao obter todos os produtos", ex);
        }
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                throw new ProductServiceException("Produto não encontrado", new Exception("Exceção interna: Produto não encontrado"));
            }

            return product;
        }
        catch (Exception ex)
        {
            throw new ProductServiceException("Erro ao obter o produto por ID", ex);
        }
    }

    public async Task AddProductAsync(Product product)
    {
        try
        {
            await _productRepository.AddAsync(product);

            _rabbitMQService.SendMessage($"Novo produto adicionado: {product.Name}");
        }
        catch (Exception ex)
        {
            throw new ProductServiceException("Erro ao adicionar o produto", ex);
        }
    }

    public async Task UpdateProductAsync(int id, Product updatedProduct)
    {
        //evitar parametros desnecessarios id nao esta sendo usado
        try
        {
            await _productRepository.UpdateAsync(updatedProduct);
        }
        catch (Exception ex)
        {
            throw new ProductServiceException("Erro ao atualizar o produto", ex);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            await _productRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new ProductServiceException("Erro ao excluir o produto", ex);
        }
    }

    // Novo método para obter o serviço RabbitMQ
    //deveria estar injetado
    public IRabbitMQService GetRabbitMQService()
    {
        return _rabbitMQService;
    }
}

//Boa solucao mas poderia ser melhorada, acredito que o ideal seria criar uma classe base para as exceptions utilizar ela, assim voce poderia tratar todas as exceptions de uma vez so
public class ProductServiceException : Exception
{
    public ProductServiceException()
    {
    }

    public ProductServiceException(string message) : base(message)
    {
    }

    public ProductServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
