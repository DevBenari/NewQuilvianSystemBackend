using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    /// <summary>
    /// Wrapper hasil operasi layanan Lifecycle Management & Onboarding.
    /// Membawa status HTTP, payload data, dan pesan deskriptif bisnis.
    /// </summary>
    public sealed class RecruitmentResult<T>
    {
        public bool Success { get; set; }
        public bool IsSuccess => Success;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage => Message;
        public List<string>? Errors { get; set; }
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

        public static RecruitmentResult<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors,
                Data = default
            };
        }

        public static RecruitmentResult<T> BadRequest(string message, List<string>? errors = null)
        {
            return Fail(message, 400, errors);
        }

        public static RecruitmentResult<T> NotFound(string message = "Data tidak ditemukan.")
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = 404,
                Message = message,
                Data = default
            };
        }

        public static RecruitmentResult<T> Conflict(string message = "Terjadi konflik data.")
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = 409,
                Message = message,
                Data = default
            };
        }

        public static RecruitmentResult<T> Forbidden(string message = "Akses ditolak.")
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = 403,
                Message = message,
                Data = default
            };
        }

        public static RecruitmentResult<T> Unprocessable(string message = "Permintaan tidak dapat diproses.")
        {
            return new RecruitmentResult<T>
            {
                Success = false,
                StatusCode = 422,
                Message = message,
                Data = default
            };
        }
    }
}
