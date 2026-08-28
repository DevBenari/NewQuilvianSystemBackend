# BE-IGD-020 — Penilaian Ulang Menolak Kunjungan yang Sudah Selesai

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-020` |
| Slice | `IGD-S01` · `EPIC IGD-03` · gelombang `MVP-0` |
| Requirement | `FR-IGD-014` |
| Keputusan | `IGD-GAP-014`, `IGD-DEC-093`, `IGD-DEC-104` |
| Kontrak | Validation `0.4.0` §2 aturan 4 — **`approved`** (teksnya tidak berubah sejak `IGD-DEC-093`) |
| Uji | `AT-IGD-088` |
| Commit dasar | `300922c` |
| Tanggal | 26 Agustus 2026 |
| **Status** | **Selesai dan terbukti lewat test. Belum di-commit.** |

---

## 1. Ringkasan

Satu baris kondisi. `EmergencyTriageService.RetriageAsync` memeriksa apakah kunjungan sudah
ditutup, tetapi hanya mengenali `Disposed` dan `Cancelled` — **bukan `Completed`**. Akibatnya
kunjungan yang sudah benar-benar selesai masih dapat dinilai ulang.

Ini **kembaran persis** cacat yang ditutup `BE-IGD-019` pada `ValidateRequestAsync`. Lubang
yang sama ada di dua tempat; `BE-IGD-019` menutup satu, task ini menutup yang kedua.

---

## 2. Kenapa dipisah menjadi task tersendiri

`BE-IGD-019` menemukan cacat ini saat mengerjakan jalur lain, dan **sengaja tidak
memperbaikinya di sana** — roadmap sudah menempatkannya sebagai `BE-IGD-020`. Memperbaiki dua
hal dalam satu task membuat diff-nya lebih sulit ditinjau, dan menghapus jejak bahwa cacat ini
punya acceptance criteria sendiri.

Yang **tidak** boleh terjadi adalah menemukannya lalu melupakannya. Karena itu `BE-IGD-019`
mencantumkannya di laporannya sendiri bagian 2.3, lengkap dengan nomor barisnya.

---

## 3. Yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Services/EmergencyTriageService.cs` baris 146–148 | `EmergencyVisitStatus.Completed` ditambahkan ke pemeriksaan kunjungan tertutup |

Pesan penolakannya **tidak diubah** — *"Kunjungan IGD sudah ditutup, sehingga tidak dapat
dinilai ulang."* sudah benar dan sudah menyebut apa yang salah.

`RetriageAsync` membaca kunjungan dengan `AsNoTracking()` dan **tidak menulis `VisitStatus`
sama sekali**. Penulisan status terjadi belakangan, ketika penilaian baru diselesaikan lewat
`Create` atau `UpdateTriageStatus` — dan kedua jalur itu sudah dijaga `BE-IGD-019`. Jadi task
ini tidak perlu menyentuh penjaga transisi.

---

## 4. Seluruh pemeriksaan status kunjungan diperiksa, bukan hanya yang di task

Mengikuti pelajaran `BE-IGD-016`. Penelusuran `VisitStatus` pada `EmergencyTriageService.cs`
menemukan **empat** tempat:

| Baris | Isi | Keadaan |
| ---: | --- | --- |
| 48–50 | `ValidateRequestAsync` — jalur create | Sudah diperbaiki `BE-IGD-019` |
| 146–148 | `RetriageAsync` — jalur penilaian ulang | **Diperbaiki task ini** |
| 263 | Pemantau pelampauan SLA | **Tidak disentuh** — lihat 4.1 |
| 322 | Daftar pantau pelampauan SLA | **Tidak disentuh** — lihat 4.1 |

### 4.1 Satu temuan yang dicatat, bukan diperbaiki

Baris 263 dan 322 menyaring kunjungan dengan `TreatmentStartedAt == null` **dan**
`VisitStatus != Cancelled`. Keduanya **sengaja** memakai `TreatmentStartedAt` alih-alih status,
dan komentarnya menjelaskan alasannya: kolom itu diisi sekali dengan `??=` dan tidak pernah
tertimpa, sehingga merupakan penanda paling langsung.

Tetapi ada celah yang belum tertutup: **kunjungan yang ditutup `Completed` tanpa penanganan
pernah dimulai** — misalnya pasien mendaftar, ditriase, lalu pergi sebelum ditangani —
`TreatmentStartedAt`-nya tetap kosong dan `VisitStatus`-nya bukan `Cancelled`. Kunjungan
seperti itu akan **terus muncul pada daftar pantau pelampauan SLA** meski kunjungannya sudah
tidak berjalan.

**Tidak diperbaiki di sini** karena berada di luar `BE-IGD-020`, dan karena aturan daftar
pantau diatur `IGD-DEC-083` — bukan aturan triase. Dicatat sebagai `IGD-OQ-080`.

---

## 5. Verifikasi

| Kriteria acceptance roadmap | Hasil |
| --- | --- |
| 1. Penilaian ulang pada kunjungan `Completed` ditolak `409` | **Ya** |
| 2. Penilaian ulang pada kunjungan `InTreatment` dan `Triaged` tetap berhasil | **Ya** — diuji juga untuk `UnderObservation` |

```
dotnet test --filter "FullyQualifiedName~EmergencyTriageVisitStatusTests"
→ Passed!  Failed: 0, Passed: 34, Skipped: 0, Total: 34

dotnet test ./QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj
→ Failed: 2, Passed: 718, Skipped: 0, Total: 720

dotnet build ./QuilvianSystemBackend.sln --configuration Release
→ Build succeeded. 0 Error(s), 152 Warning(s)
```

Suite naik **713 → 720**. Dua yang gagal adalah **dua yang sama** milik `InPatientManagement`
yang sudah gagal sejak sebelum gelombang ini dimulai. **Nol regresi.**

Test butir 2 sengaja ditulis longgar: bila penilaian ulang gagal karena sebab lain, ia hanya
memastikan kegagalannya **bukan** karena kunjungan dianggap tertutup. Yang diuji adalah
penjaga kunjungan, bukan seluruh alur penilaian ulang.

---

## 6. Yang sengaja tidak dikerjakan

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Celah daftar pantau SLA pada baris 263 dan 322 | Di luar cakupan; diatur `IGD-DEC-083`. Dicatat `IGD-OQ-080` |
| Lima titik tulis observasi, resusitasi, disposisi | **`BE-IGD-021`** |
| Penyelesaian kunjungan lewat penjaga | **`BE-IGD-022`** |
| `git commit` / `push` | Tidak diminta |
