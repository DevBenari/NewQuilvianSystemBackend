# Profil Project Backend Saat Ini

Turunkan detail implementasi final dari source saat ini dan `AGENTS.md`. Baseline saat ini: ASP.NET Core Web API di atas .NET 9, EF Core 9, PostgreSQL/Npgsql, dan `Repositories/ApplicationDbContext.cs`.

Gunakan area/domain pemilik serta controller, DTO, model, dan service matang yang terdekat. Pertahankan konvensi API bertversi serta envelope `ApiResponse<T>` dan `PagedResult<T>` yang sudah baku bila berlaku. Pertahankan `[Authorize]`, `AccessController`, `AccessAction`, `AccessPermission`, ownership current-user, dan wewenang workflow backend.

Permukaan endpoint sudah baku dan dikunci per jenis capability. Baca [master-data-endpoint-standard.md](master-data-endpoint-standard.md) untuk master data, dan [transaction-endpoint-standard.md](transaction-endpoint-standard.md) untuk transaksi, sebelum merancang route, DTO, atau filter apa pun. Keduanya berbagi permukaan baca yang sama, tetapi berbeda dalam cara mengubah data: master data menyunting field, transaksi berpindah status lewat aksi bernama.

Baca [role-access-rules.md](role-access-rules.md) sebelum menyentuh endpoint apa pun: hak akses ditentukan admin lewat layar Akses Role, dan tidak boleh di-hardcode di dalam kode.

Baca `API_RULES.md` sebelum pekerjaan API dan `DATABASE_RULES.md` sebelum pekerjaan persistence. Pekerjaan entity/source tidak pernah dengan sendirinya memberi wewenang pembuatan migration, eksekusi database, atau deployment.
