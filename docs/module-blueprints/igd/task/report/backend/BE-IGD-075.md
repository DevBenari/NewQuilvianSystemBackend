# Laporan Perubahan Backend — `BE-IGD-075`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-075` |
| Judul | Penautan invoice ditahan untuk permintaan bersumber IGD |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 gelombang 3 — **perubahan milik Billing** |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | DoD PRD §12.5 butir 7 (tagihan IGD tidak ditautkan); `FR-IGD-118` tetap `OPEN DECISION` (`IGD-OQ-129`) — task ini hanya menahan |
| Keputusan | `IGD-DEC-293` (tahan tautan invoice sumber IGD sampai `IGD-OQ-129` dijawab), `IGD-DEC-299` (persetujuan lisan owner Billing) |
| Contract version | integration **`0.8.0`** §9.1, §9.4; API **`0.17.1`** §12.6 catatan Billing |
| Dependency | `BE-IGD-073` 🟡 — kolom `InpAdmissionReferral.SourceKind` ada di source dan migration 2 sudah diterapkan di dev (14 Oktober 2026) |
| Klasifikasi | `SMALL` — satu saringan pada satu kueri baca di service Billing; nol endpoint, nol migration |
| Task mode | `BACKEND` — izin pemilik 14 Oktober 2026: *"sudah saya build dan database update, sekarang lanjutkan"* |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): satu berkas pada 3.2; laporan ini, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9e0e517f` (`rizkiG`) + working tree (`BE-IGD-073`, `074` belum di-commit) |
| Tanggal | 14 Oktober 2026 |
| Status | ⛔ **DIBATALKAN — 14 Oktober 2026, `IGD-DEC-304`.** Saringan Billing dikembalikan ke `HEAD`; `IGD-DEC-293` tetap terjaga karena jalur Transfer tidak membuat `InpAdmissionReferral` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `BillingManagement` / `Billing` |
| Pemilik dan prefix registry | Billing, prefix **`Bil`**; pemilik perubahan owner Billing, persetujuan lisan tercatat `IGD-DEC-299`. Dikerjakan agent IGD atas persetujuan itu |
| Keberlakuan | `TOUCHED LEGACY` — satu berkas lama, satu kueri. Nol berkas baru |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-MOD-001` |
| `Program.cs` | Nol baris |
| Migration | Tidak ada |

---

## 1. Masalah yang diselesaikan

Saat pembayaran rawat inap diterima, Billing menautkan kunjungan asal ke invoice rawat inap. Kunjungan asal itu diambil dari
setiap permintaan admisi `Completed` milik episode tersebut (`EnsureEncounterLinksAsync`). Sejak `BE-IGD-074`, permintaan
admisi juga lahir dari IGD dengan `SourceEncounterId` = encounter IGD. Tanpa saringan, tagihan kunjungan IGD akan ikut
tertaut ke invoice rawat inap — padahal pemilik memutuskan menahannya sampai owner Billing menjawab `IGD-OQ-129`
(`IGD-DEC-293`).

Contoh. Tn. Budi dirawat inap dari permintaan IGD. Saat kuitansi rawat inapnya diproses, encounter IGD-nya **tidak** masuk
`BilInvoiceEncounterLink`. Ny. Sari yang dirawat inap dari kamar pulih tetap mendapat tautan kunjungan OK-nya seperti
sebelumnya.

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Petugas admisi | Membuka admisi dari permintaan (kamar pulih atau IGD); permintaan menjadi `Completed` |
| 2 | Rawat Inap → Billing | Kejadian pembayaran/kuitansi rawat inap diterima `BillingInpatientEventReceiver` |
| 3 | Billing | `EnsureEncounterLinksAsync` membaca permintaan `Completed` episode itu — **hanya yang bersumber kamar pulih** — lalu membuat tautan yang belum ada |

**Jalur tidak normal.** Tidak ada penolakan baru. Permintaan bersumber IGD dilewati diam-diam; tagihan IGD tetap berdiri
sendiri dan diselesaikan lewat alur IGD.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-075`; integration §9.1, §9.4; API §12.6; `02-backend-architecture.md` §17.3; source
`BillingInpatientEventReceiver.cs` (`:100`, `:195`, `:247`–`273`), `BilInvoiceEncounterLink.cs`, `InpAdmissionReferral.cs`,
`InpAdmissionReferralStatus.cs` (enum `InpAdmissionReferralSourceKind`). Pencarian `InpAdmissionReferral` di
`Areas/HealthServices/BillingManagement` dan `Areas/Corporate`: satu-satunya pembaca permintaan admisi di Billing adalah
`EnsureEncounterLinksAsync`; `BilInvoiceEncounterLink.SourceReferral` hanya navigasi.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingInpatientEventReceiver.cs` | `EnsureEncounterLinksAsync`: saringan `x.SourceKind == InpAdmissionReferralSourceKind.RecoveryRoom` pada kueri permintaan (+2/−1). `using` tidak bertambah — namespace enum sudah diimpor |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint berubah |
| Database | Nol migration; perilaku tulis berubah: tidak ada baris `BilInvoiceEncounterLink` baru dari permintaan bersumber IGD |
| Keamanan/Auth | Tidak ada |
| Pemanggil | Kedua jalan masuk ke `EnsureEncounterLinksAsync` — kuitansi baru (`:195`) dan pemrosesan ulang kuitansi lama (`:100`) — ikut tersaring |

### 3.4 Delta terhadap kontrak yang disetujui

Tidak ada. Saringan persis seperti Cakupan kartu.

## 4. Dokumentasi endpoint

Tidak ada endpoint yang ditambah atau diubah. Perilaku berubah di dalam service penerima kejadian Rawat Inap milik Billing.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `-Mode Strict` atas 1 berkas | `VIOLATION 0`, `REVIEW 0`, `INFO 0`, *Final result: PASS* | `PASS` | 14 Oktober 2026 |
| Akhiran baris | CRLF tanpa BOM, nol LF tunggal (270 baris) | `PASS` | Hitungan byte Node |
| Komentar baru, `Program.cs` | Nol; tidak berubah | `PASS` | `git diff` |
| `dotnet build -p:RunAnalyzers=false` | Belum | `NOT RUN` | Wewenang Rizki |
| Uji API acceptance 1–2 | Belum | `NOT RUN` | Putaran B: skenario 075-S1, 075-S2 di bawah |

**Skenario uji.**

| ID | Langkah | Hasil yang diharapkan |
| --- | --- | --- |
| 075-S1 | Sesudah 074-S1, petugas admisi membuka admisi dari permintaan IGD; terima pembayaran/kuitansi rawat inap episode itu; buka ringkasan tagihan episode | Tidak ada tautan kunjungan IGD pada invoice rawat inap |
| 075-S2 | Regresi: pasien dari kamar pulih (permintaan `RecoveryRoom`) dengan alur yang sama | Kunjungan asal tetap tertaut seperti sebelum task |

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Admisi dari permintaan IGD: nol baris `BilInvoiceEncounterLink` baru untuk kunjungan IGD | Source terpenuhi; uji belum | Saringan `SourceKind` |
| 2 | Admisi dari permintaan kamar pulih tetap menautkan kunjungan asal | Source terpenuhi; uji belum | Baris lama bernilai `SourceKind = 1` (bawaan migration 2) |
| 3 | Diff satu berkas; nol komentar baru; akhiran baris benar; build 0 error | Diff, komentar, akhiran baris terpenuhi; **build belum** | Bagian 5 |

DoD: persetujuan owner Billing tercatat (`IGD-DEC-299`); laporan ini ✅; build dan uji API belum.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Bila sebelum build ini ada admisi yang sempat dibuka dari permintaan IGD dan kuitansinya diproses (build `BE-IGD-074` berjalan sejak 14 Oktober 09.00), tautannya sudah terbuat dan tidak dihapus task ini. Agent tidak menjalankan kueri untuk memeriksanya |
| Dibuka kembali | Saat owner Billing menjawab `IGD-OQ-129` (`FR-IGD-118`) |
| Risiko tersisa | Rendah |
| Perubahan sampingan | `NONE` |
| Status Git | 1 `M` di `Areas/HealthServices/BillingManagement/**`; nol stage, nol commit |
| Langkah berikutnya | Tidak ada — task dibatalkan |

## Pembatalan — 14 Oktober 2026 (`IGD-DEC-304`)

Owner menetapkan tugas IGD berakhir saat pasien masuk Antrean Transfer IGD/Poli di *Admisi Rawat Inap* milik Muhammad Hamzah. Jalur permintaan admisi bersumber IGD yang dibangun task ini tidak dipakai. `BillingInpatientEventReceiver.cs` dikembalikan dengan `git restore --source=HEAD`. Penautan invoice tetap hanya lewat `InpAdmissionReferral`; jalur Transfer tidak membuatnya, sehingga `IGD-DEC-293` terjaga tanpa perubahan Billing.

Validasi pembongkaran: `git status` menunjukkan berkas task ini kembali sama dengan `HEAD`. Dokumen kontrak yang memuat jalur lama dicatat perlu amandemen pada `IGD-DEC-304`.
