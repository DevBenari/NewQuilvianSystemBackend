# Laporan Perubahan Backend — `BE-RWI-082`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-082` |
| Judul | Penutupan episode mengunci konsep catatan dokter |
| Slice | Gelombang 2 — `RI-V2-1`, `EPIC RI-41`, langkah penutupan 4 |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-082` |
| Trace | `FR-RI-198`; `INT-INP-08`; `RWI-DEC-138`, `RWI-DEC-151`; `RM-DEC-003`, `RM-DEC-013`; `INV-INP-11`, `INV-DOK-18`; `NFR-025`; `02-backend-architecture.md` 11.5.4 langkah 4 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-091` [BE-DOK] — **belum mendarat.** Lihat bagian 6 "Dependency dan batas cakupan" |
| Klasifikasi | `MEDIUM` — satu pemanggilan lintas modul di dalam transaksi yang sudah ada, satu dependency baru pada service |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **Selesai di source.** Cakupan nyatanya bertambah sendiri ketika `BE-RWI-091` mendarat — lihat bagian 6. `dotnet build`, `UAT-50`, `UAT-51`, dan uji galat buatan **`NOT RUN`** |

---

## 1. Masalah yang diperbaiki

Ketika episode rawat inap ditutup, kadang masih ada catatan klinis yang berstatus **konsep** —
ditulis dokter tetapi belum sempat ditandatangani.

Sebelum task ini, penutupan episode tidak melakukan apa-apa terhadap konsep itu. Ia dibiarkan
terbuka selamanya pada kunjungan yang sudah tidak berjalan. Akibatnya, sebuah catatan klinis masih
dapat disunting berbulan-bulan setelah pasiennya pulang — dan tidak ada satu pun jejak yang
menandai bahwa penyuntingan itu terjadi setelah episodenya tertutup.

**Dua jalan keluar yang salah, dan kenapa keduanya ditolak.**

| Jalan keluar | Kenapa salah |
| --- | --- |
| Menghapus konsepnya | Riwayat klinis hilang. Catatan Kamis malam yang belum ditandatangani tetap memuat fakta yang terjadi Kamis malam |
| Menandatanganinya otomatis | Sistem memalsukan tanda tangan dokter. Tanda tangan berarti dokter menyatakan isinya benar; sistem tidak pernah dapat menyatakan itu atas namanya |

`RWI-DEC-138` memilih jalan ketiga, mengikuti `RM-DEC-003`: konsep itu **dikunci apa adanya** dan
ditandai "Tidak Ditandatangani".

**Contoh nyata.** Joko pulang Jumat pukul 14.00. Ada satu catatan SOAP konsep dari Kamis malam yang
ditulis dr. Yoga dan belum ditandatangani. Setelah penutupan, SOAP itu **tetap ada**, berstatus
`LockedUnsigned`, terbaca siapa penulisnya dan kapan ditulis, dan tidak dapat lagi disunting siapa
pun.

---

## 2. Proses bisnis

**Tujuan.** Konsep yang tidak sempat ditandatangani tetap terbaca sebagai konsep yang terkunci —
bukan hilang, dan bukan tertanda tangan.

**Pelaku.** Petugas yang menutup episode. Ia **tidak** perlu tahu ada konsep atau tidak; penguncian
terjadi sendiri.

**Pemicu.** Episode ditutup, baik lewat penutupan biasa maupun lewat jalan keluar supervisor.

**Langkah yang berurutan di dalam transaksi penutupan.**

| Langkah | Isi | Keadaan pada rilis ini |
| ---: | --- | --- |
| 1 | Kembalikan penempatan tempat tidur | Sudah ada sebelum task ini |
| 2 | Tutup penugasan dokter dan perawat yang masih aktif | Sudah ada sebelum task ini |
| **4** | **Konsep catatan dokter menjadi `LockedUnsigned`, `LockTrigger = EncounterClosed`** | **Dipasang task ini** |
| 5 | Batalkan pesanan tindakan tertunda yang belum ditagih | Belum terpasang — `BE-RWI-083` |
| 6 | Batalkan dosis obat berjadwal setelah waktu tutup | Belum terpasang — `BE-RWI-087` |
| 3 / 7 | `ClosedAt`, status `Closed`, riwayat status, lalu `SaveChanges` dan commit | Sudah ada sebelum task ini |

**Kenapa di dalam transaksi, bukan sesudahnya.** Penguncian yang dijalankan setelah commit akan
meninggalkan episode tertutup dengan konsep yang masih terbuka bila langkah itu gagal — dan tidak
ada yang tahu bahwa ia pernah gagal. `INV-INP-11` dan `NFR-025` menuntut sebaliknya: satu galat pada
langkah mana pun membuat **nol** perubahan tersimpan, dan episode tetap `DischargePending`.

**Kenapa lewat service pemilik, bukan menulis tabelnya langsung.** Tabel
`MrcClinicalDocumentIntegrity` milik `MedicalRecordManagement`. Menulisnya dari
`InPatientManagement` akan melewati seluruh aturan pemiliknya —
`02-backend-architecture.md` bagian 11.3. Pemanggilannya lewat
`ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync`, dan karena service itu
memakai `ApplicationDbContext` yang sama, penguncian ikut transaksi penutupan tanpa perlu
diapa-apakan.

**Aturan yang berlaku.**

| ID | Bunyinya |
| --- | --- |
| `INV-DOK-18` | `LockedUnsigned` tidak pernah kembali menjadi `Draft` |
| `INV-INP-11` | Seluruh langkah penutupan berada dalam satu transaksi |
| `NFR-025` | Galat pada langkah mana pun → nol perubahan tersimpan |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Episode tidak punya satu pun konsep | Penguncian mengembalikan `0` dan penutupan berjalan normal tanpa galat |
| Penguncian gagal di tengah jalan | Transaksi di-rollback; tempat tidur tidak dilepas, penugasan tidak ditutup, episode tetap `DischargePending` |
| Episode punya ratusan konsep | Dikunci bertahap 200 baris sekali ambil, di dalam transaksi yang sama — agar tabelnya tidak tertahan terlalu lama |

**Kenapa penguncian tidak menahan penutupan.** Banyaknya konsep tidak pernah menjadi alasan menolak
penutupan. Pasien yang sudah pulang tetapi episodenya tetap terbuka adalah keadaan yang jauh lebih
berbahaya daripada satu konsep yang terkunci tanpa tanda tangan — `RWI-DEC-138` (5). Petugas
diberi **peringatan** lebih dulu lewat endpoint kesiapan penutupan (`BE-RWI-084`), bukan
penghalang.

**Hasil akhirnya.** Setelah penutupan, jumlah konsep yang benar-benar terkunci dikembalikan pada
`sideEffects.lockedDraftCount` — angka yang benar-benar tersimpan, bukan perkiraan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `.../02-backend-architecture.md` 11.5.4 | Urutan langkah penutupan dan bentuk pemanggilannya |
| `data/data-dictionary.md` 18.5 | Penegasan bahwa `InPatientManagement` tidak memetakan tabel modul lain |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Tanda tangan `LockOpenDocumentsForEncounterAsync` dan perilakunya |
| `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs` | Bentuk registrasi keutuhan |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentIntegrityStatus.cs`, `ClinicalDocumentLockTrigger.cs` | Nilai `LockedUnsigned` dan `EncounterClosed` |
| `Areas/.../Services/InpDischargeService.Closure.cs` — `CloseEpisodeInternalAsync` | Batas transaksi dan urutan langkah yang sudah ada |
| `Program.cs` | Memastikan `ClinicalDocumentIntegrityService` sudah terdaftar scoped |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/InpDischargeService.cs` | Konstruktor menerima `ClinicalDocumentIntegrityService`; alasannya ditulis pada `<remarks>` konstruktor |
| `Areas/.../Services/InpDischargeService.Closure.cs` | Langkah 4 dipasang di dalam transaksi `CloseEpisodeInternalAsync`; jumlah konsep terkunci dibawa ke `SideEffects` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Perubahan perilaku, bukan perubahan bentuk.** `POST /{episodeId}/close` dan `/close-with-override` kini ikut mengunci konsep. Bentuk permintaannya tidak berubah; balasannya bertambah `sideEffects` yang dipasang `BE-RWI-084` |
| Database | Tidak ada perubahan schema dari task ini. **Ada perubahan data lintas modul:** penutupan episode kini menulis kolom status, `LockedAt`, `LockTrigger`, dan `LockedEncounterClosedAt` pada `MrcClinicalDocumentIntegrity` milik `MedicalRecordManagement`. Persetujuan pemanggilan ini tercatat `RWI-DEC-151`, 16 September 2026, oleh Yoga Aji Pratama |
| Keamanan/Auth | Tidak ada perubahan atribut akses. `InpatientDischarge : Close` dan `InpatientDischarge : CloseOverride` tetap. Penguncian **menyempitkan** apa yang dapat disunting, tidak melebarkan |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close` | Menutup episode. **Perilaku baru:** konsep catatan dokter ikut dikunci di dalam transaksi yang sama | `InpatientDischarge : Close` |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close-with-override` | Supervisor menutup menembus gerbang keuangan. **Perilaku baru:** sama | `InpatientDischarge : CloseOverride` |

Tidak ada endpoint baru, dan tidak ada bentuk permintaan yang berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan pemilik pekerjaan pada permintaan task ini |
| Verifikasi proses bisnis `UAT-50` dan `UAT-51` | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta database; bersandar pada build yang dikecualikan |
| Uji galat buatan pada Postgres sekali pakai — galat → nol perubahan tersimpan | Tidak dijalankan | `NOT RUN` | Alasan sama |
| Pemeriksaan batas transaksi pada source | Langkah 4 berada **di antara** `BeginTransactionAsync` dan `SaveChangesAsync`/`CommitAsync`, sejajar dengan pelepasan tempat tidur dan penutupan penugasan. Blok `catch` yang sudah ada melakukan `RollbackAsync` lalu melempar ulang | `PASS` pemeriksaan source | `InpDischargeService.Closure.cs` — `CloseEpisodeInternalAsync` |
| Pemeriksaan arah tulis lintas modul | `InPatientManagement` tidak menulis `MrcClinicalDocumentIntegrity` secara langsung; seluruhnya lewat `ClinicalDocumentIntegrityService` | `PASS` | Pencarian `MrcClinicalDocumentIntegrity` di dalam `InPatientManagement` hanya menemukan satu **pembacaan** untuk peringatan (`BE-RWI-084`), bukan penulisan |
| Pemeriksaan `INV-DOK-18` | `LockOpenDocumentsForEncounterAsync` hanya menyaring `IntegrityStatus == Draft` dan menetapkannya `LockedUnsigned`; tidak ada jalur di repository ini yang mengembalikan `LockedUnsigned` ke `Draft` | `PASS` pemeriksaan source | `ClinicalDocumentIntegrityService.cs`; pencarian penetapan `IntegrityStatus = Draft` hanya menemukannya pada pendaftaran dokumen baru |
| Persetujuan `INT-INP-08` tercatat | `RWI-DEC-151`, 16 September 2026, Yoga Aji Pratama. Tidak diperlukan pemberitahuan terpisah | `PASS` | Kartu `BE-RWI-082` pada roadmap; tabel gerbang roadmap baris pertama |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `TOUCHED LEGACY` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Review diff dan scope | Dua berkas disentuh, keduanya di dalam `InPatientManagement`. Tidak satu berkas pun di `MedicalRecordManagement` yang diubah | `PASS` | `git status --short` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang sudah dimigrasi.

**Tidak dijalankan:** `dotnet build`, `UAT-50`, `UAT-51`, dan uji galat buatan. Seluruhnya
dikecualikan pemilik pekerjaan yang menyatakan akan menjalankan build sendiri. **Uji galat buatan
adalah bukti yang diminta kartu task secara eksplisit untuk `NFR-025`, dan ketiadaannya disebut di
sini apa adanya.**

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Penutupan episode mengubah seluruh konsep encounter itu menjadi `LockedUnsigned` | Terpenuhi di source | Langkah 4 memanggil `LockOpenDocumentsForEncounterAsync(episode.EncounterId, ...)` yang menyaring seluruh baris `Draft` milik encounter itu dan menetapkannya `LockedUnsigned` |
| AC-2 — `LockedUnsigned` tidak pernah kembali menjadi `Draft` — `INV-DOK-18` | Terpenuhi di source | Ditegakkan mesin keutuhan `MedicalRecordManagement`; tidak ada jalur baru yang dibuka task ini |
| AC-3 — Galat buatan pada langkah mana pun membuat **nol** perubahan tersimpan — `NFR-025` | **Terpenuhi di source, belum terbukti runtime** | Langkah 4 berada di dalam transaksi yang sama; `catch` melakukan rollback. Uji galat buatannya `NOT RUN` |
| AC-4 — Episode yang ditutup tanpa satu pun konsep tetap tertutup normal, tanpa galat | Terpenuhi di source | `LockOpenDocumentsForEncounterAsync` keluar dari perulangan pada potongan kosong dan mengembalikan `0`; angka nol tidak pernah dijadikan syarat |

**Dependency dan batas cakupan — dibaca bersama AC-1.**

Kartu roadmap menempatkan `BE-RWI-091` [BE-DOK] sebagai prasyarat task ini, dan task itu **belum
mendarat**. Yang perlu dibedakan:

| Hal | Keadaan |
| --- | --- |
| **Mesin penguncian** — `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync` | **Sudah ada** di repository ini sebelum task ini. Langkah 4 memanggilnya apa adanya |
| **Pemanggilan dari penutupan episode** — isi task ini | **Sudah dipasang** |
| **Cakupan dokumen yang punya registrasi keutuhan berstatus `Draft`** | **Belum penuh.** `BE-RWI-091` memasang registrasi keutuhan **sejak konsep** untuk SOAP dan kajian medis rawat inap. Sampai ia mendarat, dokumen jenis itu belum tentu punya baris `Draft` untuk dikunci |

Artinya: langkah 4 **bekerja dan benar** untuk setiap registrasi `Draft` yang ada, dan **cakupan
nyatanya bertambah sendiri** ketika `BE-RWI-091` mendarat — tanpa satu baris pun perlu diubah di
sini. Tidak ada pekerjaan tersisa pada sisi `InPatientManagement`.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Pemberitahuan `INT-INP-08` kepada Yoga Aji Pratama | **Terpenuhi lebih awal** — disetujui 16 September 2026 lewat `RWI-DEC-151`; laporan ini merujuknya, tanpa pemberitahuan terpisah |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang dapat dipastikan — compiler tidak dijalankan |
| Masalah yang diketahui | Cakupan dokumen yang terkunci belum penuh sampai `BE-RWI-091` mendarat — lihat bagian 6. Ini **bukan** kekurangan source pada sisi `InPatientManagement` |
| Risiko tersisa | `NFR-025` belum terbukti runtime. Tanpa uji galat buatan, tidak ada yang memastikan rollback benar-benar membatalkan penguncian ketika langkah lain gagal |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Setelah build, jalankan `UAT-50`, `UAT-51`, dan uji galat buatan pada Postgres sekali pakai lalu tempelkan keluarannya ke bagian 5. Ketika `BE-RWI-091` mendarat, cukup jalankan ulang `UAT-50` untuk membuktikan cakupannya bertambah — tanpa perubahan source di sini |
