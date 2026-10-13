# Laporan Perubahan Backend — `BE-RWI-229`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-229` |
| Judul | Pengguguran admisi Draft mengunci baris episode sehingga pembacaan bersamaan tidak lagi jatuh 500 |
| Slice | Perbaikan `PLAN-REPAIR-EPS-009` — `FIX-EPS-009-05` |
| Roadmap | [`backend-roadmap-admisi-pendaftaran.md`](../../../roadmap/backend-roadmap-admisi-pendaftaran.md) bagian 3 dan 4.4 |
| Trace | `ISS-EPS-009-05`; `RWI-DEC-030` (pengguguran saat dibaca); indeks unik `IX_InpStatusHistory_EpisodeId_SequenceNumber` |
| Contract version | API `0.11.0` `approved` — tidak ada perubahan kontrak |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — satu berkas diubah, logika transaksi pada jalur baca yang dipakai banyak endpoint, tanpa perubahan API/database |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs`; laporan dan register blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `86cb8ef8d3660e97776096fb33fcaf7dec7c9006` (`MHamzah`) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai (source + build + QBE). Uji dua permintaan bersamaan pada PostgreSQL `NOT RUN` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `RWI-DEC-068`) |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-TXN-001` (konsistensi transaksi), `QBE-SVC-001` (logika di service) |
| Wewenang | Source: ya, satu berkas. Migration, eksekusi database, deployment: tidak |

---

## 1. Masalah yang diperbaiki

Admisi yang ditinggal dalam keadaan `Draft` melewati `DraftEpisodeExpiryHours` digugurkan saat
dibaca (`RWI-DEC-030`), bukan oleh penjadwal. Halaman daftar episode memanggil
`GET /episodes` dan `GET /episodes/summary` bersamaan; keduanya menjalankan
`ExpireDueDraftEpisodesAsync`. Ketika ada satu Draft yang lewat batas, kedua permintaan
menggugurkannya pada saat yang sama, menghitung nomor riwayat yang sama ("terakhir + 1"), dan yang
kalah ditolak indeks unik:

```text
2026-10-12T07:12:30Z  GET /episodes/summary  200  746 ms
2026-10-12T07:12:30Z  GET /episodes          500  778 ms
DbUpdateException → 23505 duplicate key value violates unique constraint
"IX_InpStatusHistory_EpisodeId_SequenceNumber"
  at InpEpisodeService.ExpireDraftIfDueAsync ← ExpireDueDraftEpisodesAsync ← GetEpisodeListAsync
```

Petugas melihat "Terjadi kesalahan pada server." pada daftar episode; memuat ulang dua detik
kemudian berhasil.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Admisi telantar gugur sendiri tepat satu kali, dengan satu baris riwayat bertanda sistem |
| Pelaku | Sistem, dipicu pembaca mana pun: daftar/ringkasan episode, detail episode, ubah/batal admisi, penugasan dokter/perawat |
| Pemicu | Episode `Draft` yang `UpdateDateTime` (atau `CreateDateTime`) + `DraftEpisodeExpiryHours` sudah lewat |
| Langkah | (1) Pemeriksaan awal tanpa kunci; bila belum lewat batas, selesai. (2) Buka transaksi. (3) **Baru:** kunci baris `InpEpisode` (`FOR UPDATE`), baca ulang nilainya, periksa ulang `Draft` dan batas waktu. (4) Batalkan pemesanan/penempatan, batalkan kunjungan yang lahir bersama episode, ubah status ke `Cancelled` dengan satu baris riwayat `ExpireDraft`. (5) Simpan dan commit |
| Jalur tidak normal | Pembaca kedua menunggu kunci pembaca pertama, lalu membaca episode yang sudah `Cancelled` → transaksi diakhiri tanpa menulis, hasil `false`, permintaan tetap 200. Episode yang disentuh petugas di antara pemeriksaan awal dan kunci (batas waktu bergeser) juga tidak digugurkan |
| Hasil akhir | Satu episode `Cancelled`, satu baris riwayat, tidak ada 500 |

Contoh: Draft `EP-2026-000231` dibuat 10-10 08.00, batas 24 jam. 12-10 14.12 dua panel membaca
bersamaan. Panel ringkasan mengunci baris dan menggugurkannya; panel daftar menunggu ±beberapa
milidetik, membaca status `Cancelled`, lalu menampilkan daftar tanpa galat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpEpisodeService.cs` (`ExpireDraftIfDueAsync`, `ApplyStatusChangeAsync`, `CancelEpisodeInternalAsync`,
`LoadEpisodeForWriteAsync`), `InpEpisodeService.Reads.cs` (`GetEpisodeListAsync`,
`GetEpisodeSummaryAsync`, `ExpireDueDraftEpisodesAsync`, `GetEpisodeDetailAsync`),
`InpEpisodeService.Assignments.cs` (empat pemanggil), `InpBedTransactionLease.cs`,
`InpBedMutationCoordinator.cs` (`LockAsync`), `InpBedOccupancyService.cs` (`LockBedRowAsync`),
`Logs/quilvian-backend-20261012.json`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | `ExpireDraftIfDueAsync` memanggil `LockDraftStillDueAsync` di dalam transaksi sebelum membatalkan; bila episode sudah tidak `Draft` atau belum lewat batas, transaksi di-rollback dan hasilnya `false`. Method baru `LockDraftStillDueAsync`: `SELECT 1 FROM public."InpEpisode" WHERE "Id" = {0} FOR UPDATE` (hanya penyedia relasional), `Entry(episode).ReloadAsync`, periksa ulang status dan batas waktu |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint, payload, atau status yang berubah; 500 lama menjadi 200 |
| Database | Tidak ada perubahan schema atau migration. Menambah satu kunci baris singkat di dalam transaksi pengguguran yang sudah ada |
| Keamanan/Auth | `NOT APPLICABLE` |

Semua pemanggil memuat episode lewat `LoadEpisodeForWriteAsync` (entity terlacak), sehingga
`ReloadAsync` memperbarui entity yang sama yang kemudian dibaca pemanggil. Tidak ada pemanggil yang
membuka transaksi sebelum memanggil method ini; bila suatu saat ada, `InpBedTransactionLease`
memakai transaksi luar dan kunci ikut dilepas saat transaksi luar selesai.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang berubah. Endpoint yang ikut terlindungi (perilakunya
sama, hanya tidak lagi jatuh 500):

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes` | Daftar episode | `InpatientEpisode : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/summary` | Ringkasan per status | `InpatientEpisode : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | `InpatientEpisode : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false -p:BaseOutputPath=<scratchpad>/be-bin/ -p:UseAppHost=false` | Build succeeded, 0 error, 284 warning, 93 detik | `PASS` | Keluaran perintah; warning pada `InpEpisodeService.cs` hanya CS1573 lama di `CancelAdmissionAsync` (baris 517–521), tidak ada warning pada baris baru |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas\HealthServices\InPatientManagement\Services\InpEpisodeService.cs -Mode Strict` | 0 violation, 0 review, 0 info | `PASS` | Keluaran perintah |
| Review diff/scope | Hanya `InpEpisodeService.cs`; tidak ada dependency, konfigurasi, atau migration | `PASS` | `git diff --stat` |
| Dua `GET` bersamaan atas satu Draft lewat batas pada PostgreSQL | Tidak dijalankan | `NOT RUN` | Butuh data Draft lewat batas di database lokal pemilik; skenario pada `PLAN-REPAIR-EPS-009` bagian 8 butir 5 |

Uji manual: `NOT FEASIBLE` — backend lokal tidak berjalan saat task dikerjakan dan eksekusi database
di luar wewenang task.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:** uji transaksi PostgreSQL bersamaan, dikecualikan atas keputusan pemilik
10 September 2026 (uji runtime dijalankan pemilik). Build ditulis ke folder scratchpad supaya
`bin/Debug` milik pemilik tidak berubah; pemilik tetap perlu build ulang sebelum `dotnet run --no-build`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (`FIX-EPS-009-05`) | Status | Bukti |
| --- | --- | --- |
| AC 1 — dua `GET /episodes` dan `GET /episodes/summary` bersamaan atas satu Draft lewat batas menghasilkan dua 200, satu baris riwayat `ExpireDraft`, episode `Cancelled` | Terpenuhi pada source; runtime `NOT RUN` | `LockDraftStillDueAsync`: kunci `FOR UPDATE` + baca ulang + periksa ulang `Draft`; pembaca kedua mengembalikan `false` tanpa menulis |
| AC 2 — pengguguran tunggal tetap menulis satu baris riwayat sistem dan melepas pemesanan | Terpenuhi | Jalur `CancelEpisodeInternalAsync` tidak berubah; hanya didahului kunci |
| AC 3 — provider InMemory tidak menjalankan perintah kunci | Terpenuhi | `if (!_dbContext.Database.IsRelational()) return true;` |
| DoD: build | Terpenuhi | Bagian 5 |
| DoD: QBE | Terpenuhi | Bagian 5 |
| DoD: uji PostgreSQL | Belum dijalankan | Dikecualikan atas keputusan pemilik |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning build naik dari garis dasar 230 (30-09) menjadi 284 karena pekerjaan modul lain; tidak ada yang berasal dari task ini |
| Masalah yang diketahui | `ISS-EPS-009-T3`: pembatalan episode yang sudah ditempatkan tidak memindahkan bed ke siklus pembersihan — di luar task ini (`FIX-EPS-009-07`) |
| Risiko tersisa | Pembaca yang bersamaan kini menunggu kunci beberapa milidetik; tidak berarti pada beban loket |
| Perubahan sampingan | `NONE` (keluaran build di scratchpad, `obj/` ikut diperbarui seperti build biasa) |
| Interupsi | `NONE` |
| Status Git | ` M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` ditambah dokumen `PLAN-REPAIR-EPS-009` (issue, plan repair, laporan `FE-RWI-243`, roadmap, traceability) |
| Langkah berikutnya | `BE-RWI-228` (pembacaan bed dan penyaring papan); pemilik: build ulang backend lalu uji bagian 8 butir 5 |
