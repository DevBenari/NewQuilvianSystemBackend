using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    /// <summary>
    /// Wrapper hasil operasi layanan Recruitment Management.
    /// Membawa status HTTP, payload data, dan pesan deskriptif bisnis.
    /// </summary>
    public sealed class RecruitmentResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static RecruitmentResult<T> Ok(T data, string message = "Operasi berhasil.")
        {
            return new RecruitmentResult<T>
            {
                Success = true,
                StatusCode = 200,
                Message = message,
                Data = data
            };
        }

        public static RecruitmentResult<T> Created(T data, string message = "Data berhasil dibuat.")
        {
            return new RecruitmentResult<T>
            {
                Success = true,
                StatusCode = 201,
                Message = message,
                Data = data
            };
        }

        public static RecruitmentResult<T> Fail(string message, int statusCode = 400)
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }

        public static RecruitmentResult<T> NotFound(string message = "Data tidak ditemukan.")
        {
            return Fail(message, 404);
        }

        public static RecruitmentResult<T> Conflict(string message)
        {
            return Fail(message, 409);
        }

        public static RecruitmentResult<T> Unprocessable(string message)
        {
            return Fail(message, 422);
        }
    }
}
