
using WEB.Common;
using WEB.Core.Extensions;

namespace WEB.Core.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Obtiene una lista de opciones para un enum, con posibilidad de incluir una opción "Todos".
        /// </summary>
        /// <typeparam name="TEnum">Tipo del enum</typeparam>
        /// <param name="includeAllOption">Si incluye una opción que representa "todos" (valor null)</param>
        /// <param name="allOptionText">Texto para la opción "Todos"</param>
        /// <returns>Lista de SelectOption:string?</returns>
        public static List<SelectOption<string?>> GetOptions<TEnum>(
            bool includeAllOption = true,
            string allOptionText = "Todos")
            where TEnum : struct, Enum
        {
            var options = new List<SelectOption<string?>>();

            if (includeAllOption)
            {
                options.Add(new SelectOption<string?> { Value = null, Text = allOptionText });
            }

            foreach (TEnum value in Enum.GetValues<TEnum>())
            {
                options.Add(new SelectOption<string?>
                {
                    Value = value.ToString(),
                    Text = value.GetDisplayName() 
                });
            }

            return options;
        }
        
        public static List<EnumCatalogItem> GetCatalog<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(e => new EnumCatalogItem
                {
                    Id = e.ToString(),         
                    Nombre = e.GetDisplayName() 
                })
                .ToList();
        }
    }
}