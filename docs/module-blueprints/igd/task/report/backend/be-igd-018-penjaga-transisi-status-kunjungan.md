# BE-IGD-018 — Penjaga Transisi Status Kunjungan yang Terpusat

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-018` |
| Slice | `IGD-S01` — status kunjungan tidak dapat mundur |
| Epic | `EPIC IGD-03` · gelombang `MVP-0` |
| Requirement | `FR-IGD-015` (fondasi) |
| Keputusan | `IGD-CONF-05`, `IGD-DEC-093`, `IGD-DEC-094` |
| Kontrak | State `0.3.0` bagian 1, 1.1, 1.2 — **`approved`**, hash `a41efd8d…` |
| Uji | `AT-IGD-089` sebagian |
| Repository | `NewQuilvianSystemBackend` |
| Commit dasar | `300922c` |
| Tanggal | 26 Agustus 2026 |
| **Status** | **Selesai dan terbukti lewat test. Belum di-commit.** |

---

## 1. Ringkasan

Menambahkan satu metode penjaga, `EmergencyVisitService.TryApplyVisitStatus`, sebagai
satu-satunya jalan yang dibenarkan untuk mengubah `TrxEmergencyVisit.VisitStatus`.

**Nol pemanggil diubah pada task ini.** Perilaku aplikasi belum bergerak sedikit pun — yang
bertambah hanya kemampuan, ditambah 168 test yang mengunci kontraknya. Pemindahan pemanggil
ke penjaga ini adalah `BE-IGD-019`, `BE-IGD-021`, dan `BE-IGD-022`.

Alasan memisahkannya: pemindahan tujuh titik tulis sekaligus dengan pembuatan penjaganya
membuat satu diff yang tidak dapat ditinjau. Bila penjaga salah, seluruh jalur klinis salah
bersamaan.

---

## 2. Latar: `IGD-CONF-05`

Penelusuran `visit.VisitStatus =` pada `Areas/HealthServices/EmergencyInstallationManagement`
menemukan **sembilan titik tulis di lima controller**. Hanya **satu** yang melewati
pemeriksaan transisi.

| Berkas | Baris | Menulis | Penjagaan sebelum task ini |
| --- | ---: | --- | --- |
| `EmergencyTriageController.cs` | 250 | `Triaged` | **Tidak ada** |
| `EmergencyTriageController.cs` | 356 | `Triaged` | **Tidak ada** — yang diperiksa `TriageStatus`, bukan `VisitStatus` |
| `EmergencyObservationController.cs` | 277 | `UnderObservation` | **Tidak ada** |
| `EmergencyObservationController.cs` | 279 | `AwaitingDisposition` | **Tidak ada** |
| `EmergencyObservationController.cs` | 283 | `InTreatment` | **Tidak ada** |
| `EmergencyResuscitationController.cs` | 295 | `InTreatment` | **Tidak ada** |
| `EmergencyDispositionController.cs` | 335 | `Disposed` | **Tidak ada** |
| `EmergencyVisitController.cs` | 378 | dari request | `CanTransition` baris 373 — sudah benar |
| `EmergencyVisitController.cs` | 433 | `Completed` | Aturan bisnis `ValidateVisitClosureAsync`, bukan matriks transisi |

Bukti ini dikumpulkan pada `f69e9e48` dan **diperiksa ulang pada `300922c`** sebelum task
dimulai, karena merge "Hamzah, Ikbal, Yasmina" mendarat di antaranya. Nomor barisnya identik.
Diperiksa pula bahwa **tidak satu pun modul lain** menulis status kunjungan IGD:
`EmergencyVisitStatus` nol pemakai di luar IGD, dan `TrxEmergencyVisit` hanya disinggung dua
model master data sebagai navigasi.

---

## 3. Yang dikerjakan

### 3.1 `EmergencyVisitService.TryApplyVisitStatus`

Ditempatkan tepat setelah `CanTransition(EmergencyVisitStatus, EmergencyVisitStatus)` yang
dipakainya.

```csharp
public bool TryApplyVisitStatus(
    TrxEmergencyVisit visit,
    EmergencyVisitStatus target,
    Guid actorUserId,
    DateTime now,
    out string? penolakan)
```

Perilakunya:

| Keadaan | Yang terjadi |
| --- | --- |
| Transisi sah dan status berubah | Menulis `VisitStatus`, `UpdateDateTime`, `UpdateBy` sekaligus; `penolakan` `null`; balik `true` |
| Transisi sah tetapi status sama | Balik `true`, **jejak audit tidak digerakkan** — tidak ada yang berubah, jadi tidak ada yang perlu dicatat |
| Transisi tidak sah | Balik `false`, `penolakan` berisi pesan, **nol field disentuh** |
| `visit` `null` | `ArgumentNullException` |

Tiga keputusan desain yang perlu diketahui peninjau:

1. **`CanTransition` tidak diubah sama sekali.** Matriksnya sudah cocok dengan tabel kontrak
   bagian 1, sel demi sel. Task ini hanya membungkusnya.
2. **Penjaga tidak memanggil `SaveChangesAsync`.** Penyimpanan tetap milik pemanggil, supaya
   perubahan status ikut transaksi yang sama dengan perubahan lain di jalurnya.
3. **Pesan penolakannya umum**, berbentuk `"Status kunjungan tidak dapat berubah dari {dari}
   ke {ke}."` Pemanggil yang kontraknya menuntut pesan khusus — jalur triase pada
   validation-matrix bagian 2 aturan 5 — cukup mengabaikan `penolakan` dan menyusun pesannya
   sendiri. Ini menjaga penjaga tetap dapat dipakai ulang lima jalur berbeda.

### 3.2 Test

`QuilvianSystemBackend.Tests/HealthServices/EmergencyInstallationManagement/EmergencyVisitStatusTransitionTests.cs`

Tabel kontrak bagian 1 disalin ke dalam test sebagai data, lalu **seluruh sembilan kali
sembilan sel** dijalankan dua kali — sekali terhadap `CanTransition`, sekali terhadap
`TryApplyVisitStatus`. Ditambah enam pemeriksaan perilaku: penulisan jejak audit, penolakan
yang tidak menyentuh apa pun, kunjungan `Completed` yang tidak dapat dibuka kembali ke
sembilan status mana pun, idempotensi status sama, isi pesan penolakan, dan `visit` `null`.

**168 test.**

Letaknya di `HealthServices/EmergencyInstallationManagement/` mengikuti dua tetangga
terdekatnya, `HealthServices/OperatingRoomManagement` dan `HealthServices/PharmacyManagement`.
Roadmap semula menulis `EmergencyInstallationManagement/` di akar folder test; path ini lebih
konsisten dengan struktur `Areas/` project utama, dan roadmap sudah disesuaikan.

---

## 4. Satu ketidakcocokan kontrak yang sengaja dibiarkan

Diagonal tabel kontrak bagian 1 tergambar `—` untuk **seluruh** status, yang bila dibaca
harfiah berarti transisi ke status yang sama selalu ditolak. Tetapi bagian 1.2 hanya menyebut
satu: *"`Completed` bersifat final; `Completed` → `Completed` pun ditolak."*

Kode menerima transisi ke status yang sama sebagai tindakan idempoten. Task ini **mengikuti
kode**, dan test mengunci perilaku itu secara eksplisit beserta alasannya.

Bila Product/Domain Owner menghendaki seluruh diagonal ditolak, itu **perubahan kontrak**,
bukan perbaikan test — dan akan mengubah perilaku setiap jalur yang menulis status berulang.
Dicatat di sini agar tidak diam-diam berubah arti di kemudian hari.

---

## 5. Verifikasi

| Kriteria acceptance roadmap | Hasil |
| --- | --- |
| 1. `CanTransition` tidak diubah | **Ya** — nol baris disentuh |
| 2. Test menutup seluruh sel tabel kontrak: ✓ diterima, — ditolak | **Ya** — 81 sel × 2 teori |
| 3. `Completed` → `Completed` ditolak | **Ya** — lewat matriks dan satu `Fact` tersendiri |
| 4. Transisi ke status sama pada status non-`Completed` diterima | **Ya**, sesuai perilaku kode yang berlaku |

Perintah dan keluarannya:

```
dotnet test --filter "FullyQualifiedName~EmergencyVisitStatusTransitionTests"
→ Passed!  Failed: 0, Passed: 168, Skipped: 0, Total: 168

dotnet test ./QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj
→ Failed:  2, Passed: 684, Skipped: 0, Total: 686

dotnet build ./QuilvianSystemBackend.sln --configuration Release
→ Build succeeded. 0 Error(s), 150 Warning(s)
```

Suite naik dari **518 menjadi 686** test. Dua yang gagal adalah **dua yang sama** yang sudah
gagal sebelum task ini: `InpStatusHistoryAndMonitoringTests` dan
`InpCorrectionAndNewbornTests`, keduanya milik `InPatientManagement`. Keduanya kegagalan
asersi perilaku bisnis, tidak bersinggungan dengan status kunjungan IGD. **Nol regresi.**

---

## 6. Yang sengaja tidak dikerjakan

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Memindahkan tujuh titik tulis ke penjaga | `BE-IGD-019` (triase), `BE-IGD-021` (observasi, resusitasi, disposisi), `BE-IGD-022` (penyelesaian kunjungan) |
| Menyentuh `CanTransition` | Acceptance melarangnya; matriksnya sudah benar |
| Memvalidasi `VisitStatus` saat pembuatan kunjungan (`EmergencyVisitController.cs:214`) | Pembuatan bukan transisi. Tidak ada aturan kontrak yang mengaturnya, dan mengarangnya di luar wewenang task |
| Memperbaiki dua test Rawat Inap yang gagal | Milik `InPatientManagement`. Diserahkan kepada Muhammad Hamzah |
| `git commit` / `push` | Tidak diminta |

---

## 7. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` | +48 baris — satu metode dan dokumentasinya |
| `QuilvianSystemBackend.Tests/HealthServices/EmergencyInstallationManagement/EmergencyVisitStatusTransitionTests.cs` | Baru |
| `docs/module-blueprints/igd/roadmap/backend-roadmap.md` | Bukti `BE-IGD-018`, metadata gate |
| `docs/module-blueprints/igd/roadmap/requirement-traceability.md` | Bukti `FR-IGD-015` |
| `docs/module-blueprints/igd/00-interview-decisions.md` | `IGD-DEC-094` |

Nol migration. Nol perubahan endpoint. Nol perubahan bentuk response.
