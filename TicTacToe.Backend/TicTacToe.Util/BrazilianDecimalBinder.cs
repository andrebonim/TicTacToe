using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

public class BrazilianDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProvider = bindingContext.ValueProvider;
        var valueResult = valueProvider.GetValue(bindingContext.ModelName);
        var rawValue = valueResult.FirstValue?.Trim();

        if (string.IsNullOrEmpty(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // Aceita tanto "10.00" (invariant) quanto "10,00" / "1.234,56" (pt-BR)
        string cleaned = rawValue
            .Replace(".", "")   // remove milhar pt-BR
            .Replace(",", "."); // troca decimal pt-BR por ponto invariant

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Formato inválido: {rawValue}");
        }

        return Task.CompletedTask;
    }
}

public class BrazilianDecimalBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
            return new BrazilianDecimalModelBinder();
        return null;
    }
}