# Permintaan Persetujuan Pemegang Modul Registrasi — Titik Sentuh IGD pada `patient-encounters`

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-002` |
| `tanggal` | 2026-10-03 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | Pemegang modul Registration Management — **nama diisi pengaju saat mengirim**: ______________________ |
| `rujukan` | PRD `04-prd-to-mvp.md` §8.6 butir 8; kontrak API `0.11.0` §8.1 dan §8.2, integration `0.4.0` §5 (`IGD-DEC-157`); laporan [`BE-IGD-053`](../task/report/backend/BE-IGD-053.md) dan [`BE-IGD-059`](../task/report/backend/BE-IGD-059.md); [audit kesiapan `MVP-7`](../evidence/2026-10-03-kesiapan-mvp-7.md) syarat C2 |
| `status` | ✅ **`disetujui` — `IGD-DEC-181`, 3 Oktober 2026.** Butir 1–5 bagian 3 disetujui secara lisan oleh pemegang dan pengembang modul Registrasi (tim Rizki Gunawan). Pemilik IGD memutuskan konfirmasi tertulis dan pencatatan nama tidak disyaratkan untuk tim internal. Butir 4 bagian 4 (pemilik layar loket) tidak dijawab — keadaan sekarang tetap. Pembatasan `DELETE` ikut disetujui untuk direncanakan sebagai task baru. *Sebelumnya:* **`disetujui lisan` — menunggu konfirmasi tertulis.** Pengaju melaporkan pada 3 Oktober 2026 bahwa pemegang modul Registrasi dan pengembang Registrasi menyetujui perubahan ini secara **lisan**. Nama keduanya dan tanggal persetujuan belum tercatat. PRD §8.6 butir 8 meminta persetujuan **tertulis beserta nama**, sehingga keputusan baru pada decision log ditulis sesudah konfirmasi tertulis ada. *Sebelumnya:* `disiapkan` — belum dikirim |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Modul IGD mengubah cara pasien gawat darurat didaftarkan: loket kini **hanya membuat encounter**, dan kunjungan IGD
baru lahir saat perawat menekan *Mulai Triage* atau *Tangani Segera*. Supaya satu pasien tidak pernah punya dua episode
IGD terbuka, penjaganya dipasang di **pintu encounter milik modul Registrasi** — endpoint `patient-encounters`. Ada
**dua berkas Registrasi** yang diubah, dan seluruh perubahan **hanya berlaku untuk encounter bertipe `Emergency`**;
rawat jalan dan tipe lain tidak tersentuh. Perubahan ini dikerjakan di bawah izin remediasi teknis `IGD-DEC-135` dan
sudah berjalan di lingkungan dev sejak 1 Oktober 2026. Yang kami minta: **persetujuan tertulis** sebelum dirilis ke
lingkungan bersama dan produksi.

---

## 2. Berkas Registrasi yang diubah

| Berkas | Bagian | Task |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | `CreateEncounterCoreAsync`, `UpdateEncounterStatus`, `CancelEncounter` | `BE-IGD-053`, `BE-IGD-059` |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | `PatientEncounterCreateRequest` + ruas `DuplicateEpisodeOverrideReason` (`string?`) | `BE-IGD-053` |

**Tidak** ada kolom baru pada `RegPatientEncounter` (berkas modelnya tidak disentuh sama sekali), **tidak** ada
migration pada tabel Registrasi, **tidak** ada route baru, dan **tidak** ada baris baru di `Program.cs`. Catatan
pendaftaran ganda beralasan disimpan di tabel milik IGD (`EmgDuplicateEpisodeOverride`).

---

## 3. Perubahan perilaku yang perlu disetujui

`[Tags("Health Services / Registration Management / Patient Encounter")]` — base URL
`api/v1/health-services/registration-management/patient-encounters`

| No | Method | Endpoint | Perilaku baru — **hanya `EncounterType = Emergency`** | Contoh |
| ---: | --- | --- | --- | --- |
| 1 | `POST` | `/` | Encounter Emergency **tidak lagi membuat antrean** (`TrxQueue`) | Pasien IGD tidak muncul di antrean poliklinik |
| 2 | `POST` | `/` | Pendaftaran ditolak **`409`** bila pasien masih punya episode IGD terbuka (encounter Emergency belum berakhir, atau kunjungan IGD masih berjalan), **kecuali** petugas mengisi `duplicateEpisodeOverrideReason` | Pak Rayyan sudah *Menunggu Triage* sejak 09.35; petugas kedua mendaftarkannya lagi pukul 09.40 tanpa alasan → `409` menyebut nomor encounter yang ada |
| 3 | `POST` | `/` | Dua pendaftaran serentak untuk pasien yang sama diantrekan dengan kunci per pasien (`pg_advisory_xact_lock`) di dalam transaksi | Dua petugas menekan simpan bersamaan → satu `200`, satu `409`; satu encounter terbentuk |
| 4 | `PATCH` | `/{id}/status` | Ditolak **`409`** untuk encounter Emergency — status encounter IGD kini diikutkan otomatis oleh kunjungan IGD (selesai/batal/pergi sebelum ditriage) | Mengubah encounter IGD menjadi `Completed` lewat jalur umum → `409`, status tidak berubah |
| 5 | `PATCH` | `/{id}/cancel` | Encounter Emergency hanya dapat dibatalkan **sebelum** kunjungan IGD-nya lahir; sesudah itu `409` dengan arahan membatalkan lewat kunjungan IGD | Encounter yang sudah punya kunjungan `IGD-…` → `409` *"Batalkan lewat kunjungan IGD tersebut; encounter akan ikut dibatalkan."* |

Aturan "episode terbuka" dimiliki IGD (`EmergencyEpisodeRule`); modul Registrasi hanya memanggilnya
(`IGD-DEC-139`, `144`, `145`, `146`, `153`).

**Bukti uji** (lingkungan dev, bukti mentah diperiksa): `BE-IGD-053` S1–S12, termasuk S4 serentak `[200, 409]` dan S6
klien tanpa pra-cek `409`; `BE-IGD-059` S1–S8, termasuk S6 serentak. Uji layar loket `036-U1`, `036-U9`, `038-U2`
pada hasil build 3 Oktober 2026.

---

## 4. Yang perlu diketahui pemegang modul Registrasi

| Butir | Isi |
| --- | --- |
| Bila tim Registrasi mengubah `CreateEncounterCoreAsync`, `UpdateEncounterStatus`, atau `CancelEncounter` | Pertahankan pemanggilan `EmergencyEpisodeRule` dan cabang khusus Emergency; tanpa itu pendaftaran ganda IGD kembali terbuka |
| `DELETE /patient-encounters/{id}` | **Belum** dibatasi: masih dapat menghapus encounter IGD yang sudah punya kunjungan — di luar task ini (laporan `BE-IGD-059`, baris *Masalah yang diketahui*). Diusulkan menjadi pekerjaan lanjutan bila pemegang modul setuju |
| Urutan rilis | Penjaga ini wajib dirilis **sesudah** rekonsiliasi encounter IGD lama dijalankan di lingkungan tujuan (`BE-IGD-052`), supaya pasien dengan encounter lama tertinggal terbuka tidak ikut ditolak |
| Layar loket IGD | Berada di folder frontend `registration-management/emergency-registration` dan diubah oleh task IGD (`FE-IGD-014`, `029`, `034`, `036`, `038`). Mohon dikonfirmasi apakah layar ini dianggap milik IGD atau Registrasi |

---

## 5. Jawaban yang diminta

Mohon pilih satu dan kirim kembali secara tertulis (pesan, email, atau tanda tangan pada berkas ini):

| Pilihan | Arti |
| --- | --- |
| ☐ **Setuju** | Butir 1–5 bagian 3 disetujui apa adanya |
| ☐ **Setuju dengan catatan** | Catatan: ______________________________________________ |
| ☐ **Tidak setuju** | Alasan dan butir yang ditolak: __________________________ |

| Field | Isi |
| --- | --- |
| Nama pemegang modul Registrasi | |
| Jabatan / peran | |
| Tanggal | |
| Jawaban atas butir 4 (pemilik layar loket IGD) | ☐ IGD ☐ Registrasi |

Sesudah jawaban diterima, IGD mencatatnya sebagai keputusan baru pada `00-interview-decisions.md`, memperbarui kolom
`owners` pada `blueprint-manifest.md`, dan menandai PRD §8.6 butir 8 terpenuhi.
