using Chloe.Entity;
using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chloe.Infrastructure
{
    public static class DbConfiguration
    {
        public static void UseTypeBuilders(params IEntityTypeBuilder[] entityTypeBuilders)
        {
            EntityTypeContainer.UseBuilders(entityTypeBuilders);
        }
        public static void UseTypeBuilders(params Type[] entityTypeBuilderTypes)
        {
            EntityTypeContainer.UseBuilders(entityTypeBuilderTypes);
        }
        public static void UseEntities(params TypeDefinition[] entityTypeDefinitions)
        {
            EntityTypeContainer.Configure(entityTypeDefinitions);
        }

        public static void UseInterceptors(params IDbCommandInterceptor[] interceptors)
        {
            if (interceptors == null)
                return;

            foreach (var interceptor in interceptors)
            {
                if (interceptor == null)
                    continue;

                DbInterception.Add(interceptor);
            }
        }
    }
}
