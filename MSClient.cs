using Confiti.MoySklad.Remap.Api;
using Confiti.MoySklad.Remap.Client;
using Confiti.MoySklad.Remap.Entities;
using Confiti.MoySklad.Remap.Queries;

namespace WBImport
{
    public static class MSClient
    {
        #region Methods

        public static async IAsyncEnumerable<Demand> GetDemandsAsync(DateTime? momentFrom = null)
        {
            var settings = Settings.Default?.MoySklad;

            if (string.IsNullOrEmpty(settings?.AccessToken))
                throw new InvalidOperationException("MoySklad access token was empty.");

            var moySkladApi = new MoySkladApi(new MoySkladCredentials
            {
                AccessToken = settings.AccessToken
            }, Defaults.HttpClient);

            var query = new ApiParameterBuilder<DemandQuery>();

            query.Limit(100);

            if (momentFrom.HasValue)
                query.Parameter("moment").Should().BeGreaterOrEqualTo(momentFrom.Value.ToString(Defaults.DATE_TIME_FORMAT));

            query.Parameter("name").Should().StartsWith("WB");

            query.Expand()
                .With(x => x.Positions).And
                .With("positions.assortment").And
                .With("positions.assortment.product");
            query.Order().By("moment");

            if (!string.IsNullOrEmpty(settings.SalesChannelId))
                query.Parameter("salesChannel").Should().Be($"https://api.moysklad.ru/api/remap/1.2/entity/store/{settings.SalesChannelId}");

            var offset = 0;

            ApiResponse<EntitiesResponse<Demand>> response = null;
            do
            {
                query.Offset(offset);

                response = await moySkladApi.Demand.GetAllAsync(query);

                var payload = response?.Payload;

                if (payload?.Meta == null)
                    break;

                if (payload.Rows == null || payload.Rows.Length == 0)
                    break;

                offset += payload.Meta.Limit;

                foreach (var row in payload.Rows)
                    yield return row;
            }
            while (!string.IsNullOrEmpty(response.Payload.Meta.NextHref));
        }

        #endregion Methods
    }
}