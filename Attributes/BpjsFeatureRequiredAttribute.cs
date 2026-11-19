using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Features;

namespace QuilvianSystemBackend.Attributes
{
    /// <summary>
    /// Menolak operasi BPJS baru (HTTP 409) selama fitur BPJS nonaktif (<see cref="IBpjsFeatureGate"/>). Dipasang pada aksi, atau pada
    /// controller dengan <see cref="AllowReads"/> agar pembacaan riwayat tetap berjalan. Tidak mengubah data apa pun dan tidak
    /// menggantikan pemeriksaan izin: urutannya, izin diperiksa lebih dulu oleh filter akses, lalu gerbang fitur.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BpjsFeatureRequiredAttribute : TypeFilterAttribute
    {
        public BpjsFeatureRequiredAttribute() : base(typeof(BpjsFeatureRequiredFilter))
        {
            Arguments = new object[] { false };
        }

        private bool _allowReads;

        /// <summary>True: metode baca (GET, HEAD, OPTIONS) lolos meski fitur nonaktif; hanya operasi tulis yang ditolak.</summary>
        public bool AllowReads
        {
            get => _allowReads;
            set
            {
                _allowReads = value;
                Arguments = new object[] { value };
            }
        }
    }

    public sealed class BpjsFeatureRequiredFilter : IAsyncActionFilter
    {
        private readonly IBpjsFeatureGate _gate;
        private readonly bool _allowReads;

        public BpjsFeatureRequiredFilter(IBpjsFeatureGate gate, bool allowReads)
        {
            _gate = gate;
            _allowReads = allowReads;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_gate.IsEnabled)
            {
                await next();
                return;
            }

            var method = context.HttpContext.Request.Method;
            var isRead = HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);
            if (_allowReads && isRead)
            {
                await next();
                return;
            }

            context.Result = new ObjectResult(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, _gate.DisabledMessage))
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }
    }
}
