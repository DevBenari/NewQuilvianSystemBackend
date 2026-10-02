# Traceability Requirement — Integrasi Rawat Inap ↔ Billing, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Berkas | `integrasi-billing/roadmap/requirement-traceability-finishing.md` — revision `1` |
| Status | **`DRAFT`**, mengikuti status kedua roadmap pendamping |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `integrasi-billing`, kontrak `1.1.0` `approved` 2026-10-02 (`RWI-DEC-221`) |
| Roadmap | `backend-roadmap-finishing.md` revision `1` (`BE-RWI-146` s.d. `159`); `frontend-roadmap-finishing.md` revision `1` (`FE-RWI-166` s.d. `171`) |
| Sumber requirement | `PRD-RWI-FINISHING-001` v`0.4` bagian 6 (rumusan FR); `04-prd-to-mvp.md` bagian 8; decision log revision `32` |
| Masukan dan hash | Seperti metadata roadmap; hash lengkap pada `../blueprint-manifest.md` bagian 2.1 |
| Source SHA | Backend `bf5c6bde`; frontend `f74758af5` |
| Gate | `evidence/02-requirement-completeness-gate.md` revision `1.10` bagian 19 dan koreksi 19.12 |

**Cara membaca bukti.** Untuk backend, bukti mengikuti `rules/backend/TEST_POLICY.md`: verifikasi API/kontrak, pencarian kode (statis), verifikasi proses bisnis dengan contoh berangka, dan verifikasi runtime bila lingkungan tersedia — **bukan** automated test. Kolom "Jenis test" pada `testing/acceptance-test-matrix.md` bagian 4 dibaca sebagai skenario verifikasi. Untuk frontend, bukti adalah lint, build, dan verifikasi manual kontrol interaktif. Butir yang tidak dijalankan ditulis `NOT RUN` di laporan task, tidak pernah dianggap lulus.

## 1. Matriks requirement → desain → kontrak → task → bukti

| Requirement | Keputusan | Desain | Kontrak `1.1.0` | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| `FR-RWF-001` Pengubah status kepulangan wajib login dan permission; webhook dihapus | `RWI-DEC-166`, `167` | Backend 9.6 | API 3.1, 3.2; permission 5 | `BE-RWI-146` | `FE-RWI-167` (pemanggil lama dicabut) | Route lama 404, tanpa token 401 (`AC-RWF-001`, `UAT-RWF-02`) | Belum dikerjakan |
| `FR-RWF-002` Status izin kasir hanya disimpan Billing | `RWI-DEC-167` | Backend 9.6; integrasi 4.3 | API 3.3 | `BE-RWI-152`, `BE-RWI-153` (`InpatientClearanceGateService` dihapus) | `FE-RWI-166` | Pencarian kode: nol pembaca/penulis `InpEpisode.ClearanceStatus` (`AC-RWF-005`) | Belum dikerjakan |
| `FR-RWF-003` Layar menampilkan status kasir paling basi 10 detik, tanpa rupiah | `RWI-DEC-160`, `167` | FE 6.3 `FE-INT-06` | API 3.3 | `BE-RWI-152` | `FE-RWI-166`, `FE-RWI-168` | Manual: setuju pukul T tampil ≤ T + 10 detik (`AC-RWF-003`) | Belum dikerjakan |
| `FR-RWF-004` Tanda keuangan manual jadi riwayat baca saja | `RWI-DEC-102` (d), `167` | Backend 9.6 | API 3.1 (`POST financial-clearance` dihapus), 3.2 | `BE-RWI-152` | `FE-RWI-166` (`FE-INP-08` baca saja) | API: `POST` 404, `GET` tetap; pencarian kode frontend | Belum dikerjakan |
| `FR-RWF-005` Penutupan tanpa izin kasir hanya dengan permission dan alasan, tanpa PIN | `RWI-DEC-187` | State 5; validasi 2 | API 3.2 | `BE-RWI-146`, `BE-RWI-153` | `FE-RWI-168` | API 403/400/200 (`AC-RWF-002`, `008`); runtime `UAT-RWF-12` | Belum dikerjakan |
| `FR-RWF-006` Keluar ruangan tidak ditahan kasir; bed lepas seketika | `RWI-DEC-186` | State 5 | API 3.2 | `BE-RWI-153` | `FE-RWI-167` | API 409 lalu 200, bed `Available` (`AC-RWF-006`); manual dialog | Belum dikerjakan |
| `FR-RWF-007` Peringatan sekali klik dan jejak status kasir saat keluar | `RWI-DEC-186` | State 5 | API 3.2, 3.5 | `BE-RWI-153` | `FE-RWI-167`, `FE-RWI-169` | Billing mati → `Unreadable` tersimpan (`AC-RWF-007`); daftar pulang sebelum izin | Belum dikerjakan |
| `FR-RWF-008` Penutupan normal wajib `CLEARED`; penguncian ulang otomatis | `RWI-DEC-186` (4) | State 5 | API 3.2 | `BE-RWI-153` | `FE-RWI-168` | 422 `INP-CLS-010`/`011` (`AC-RWF-004`); runtime `UAT-RWF-03` | Belum dikerjakan |
| `FR-RWF-010` Invoice `RANAP` terbentuk otomatis saat `Admitted` | `RWI-DEC-156`, `166`, `192` | Backend 9.6 | API 3.10; integrasi 4.2 | `BE-RWI-150` | — | Proses bisnis: satu invoice < 1 menit (`AC-RWF-010`); `UAT-RWF-01`, `11` | Belum dikerjakan |
| `FR-RWF-011` Layanan klinis rawat inap masuk invoice lewat jembatan folio | `RWI-DEC-192`, `195`, `210` | Backend 9.6 | Integrasi 4.5 | `BE-RWI-155`, `BE-RWI-158` | — | Proses bisnis per jenis layanan (`AC-RWF-011`, `090`, `091`, `012`); `UAT-RWF-23` | Belum dikerjakan |
| `FR-RWF-012` Tarif kamar dihitung Billing dari penempatan | `RWI-DEC-166` (3) | Backend 9.6 | Integrasi 4.2 | `BE-RWI-150`, `BE-RWI-155` | `FE-RWI-171` | Contoh berangka hunian 2 hari 5 jam kelas 2 (`AC-RWF-013`) | Belum dikerjakan |
| `FR-RWF-013` Hitungan tarif kamar kedua dipensiunkan | `RWI-DEC-192` (e) | Backend 9.6 | API 3.1 | `BE-RWI-147` | — | Pencarian kode: service lama tidak ada, nol tarif tertanam (`AC-RWF-014`) | Belum dikerjakan |
| `FR-RWF-014` Outbox jujur: daftar putih, `Published` hanya dengan tanda terima | `RWI-DEC-161`, `166`; `INV-RWF-05` | Backend 9.6, 9.11 | Integrasi 4.2; data 6.5, 6.7 | `BE-RWI-149`, `BE-RWI-150`, `BE-RWI-151` | — | Billing mati → `Failed` lalu `Published` sekali (`AC-RWF-016`); payload daftar putih (`AC-RWF-017`) | Belum dikerjakan |
| `FR-RWF-015` Biaya administrasi termasuk invoice yang baru berisi tarif kamar | `RWI-DEC-192` (c) | Backend 9.6 | API 3.9 | `BE-RWI-155` | `FE-RWI-171` | Contoh berangka biaya admin (`AC-RWF-013`) | Belum dikerjakan |
| `FR-RWF-016` Kegagalan Billing tidak menggagalkan penyimpanan klinis | `RWI-DEC-161`, `166` | Backend 9.6 | Integrasi 4.2 | `BE-RWI-150`, `BE-RWI-151` | — | Billing mati; simpan tindakan; Billing hidup → satu baris (`AC-RWF-015`) | Belum dikerjakan |
| `FR-RWF-017` Putar ulang terkontrol saat rilis | `RWI-DEC-169` | Backend 9.6 | API 3.6 | `BE-RWI-157` | `FE-RWI-171` | Lingkungan uji: putar ulang dua kali (`AC-RWF-018`, `UAT-RWF-15`). Produksi (`I5`) butuh wewenang tertulis | Belum dikerjakan |
| `FR-RWF-018` Label `RANAP` seragam; tagihan susulan mencabut izin | `RWI-DEC-192` (h) | Backend 9.6 | — | `BE-RWI-147`, `BE-RWI-153` | — | Izin `CLEARED` lalu obat susulan → `REVOKED`, penutupan ditolak (`AC-RWF-019`) | Belum dikerjakan |
| `FR-RWF-019` Koreksi salah catat penempatan tanpa tarif dobel | `RWI-DEC-157`, `192` (g) | State 5; `RSK-RWF-02` | API 3.4 | `BE-RWI-149`, `BE-RWI-154` | `FE-RWI-170` | Dua uji wajib `IsSuperseded` dengan Billing sungguhan; 422 `INP-COR-001` (`UAT-RWF-25`) | Belum dikerjakan |
| `FR-RWF-020` Rincian per kelompok V1 | `RWI-DEC-170` (1) | Backend 9.11 | API 3.7 | `BE-RWI-156` | Layar: `keperawatan` `FE-RWI-185` | Bentuk respons `breakdown` | Belum dikerjakan |
| `FR-RWF-021` Baris tanpa rupiah: nama, periode, jumlah | `RWI-DEC-170` (2) | — | API 3.7 | `BE-RWI-156` | `keperawatan` `FE-RWI-185` | Bentuk respons | Belum dikerjakan |
| `FR-RWF-022` Subtotal dan total hanya bagi pemegang izin rupiah | `RWI-DEC-170` (3) | — | API 3.7 | `BE-RWI-148`, `BE-RWI-156` | `keperawatan` `FE-RWI-185` | `/breakdown/amounts` 403 bagi perawat (`AC-RWF-020`, `021`) | Belum dikerjakan |
| `FR-RWF-023` Sumber rincian hitungan invoice Billing | `RWI-DEC-170` (5) | Backend 9.6 | API 3.7 | `BE-RWI-156` | — | Review diff: tidak ada hitung ulang di Rawat Inap | Belum dikerjakan |
| `FR-RWF-024` Rupiah disaring di server berdasarkan permission | `RWI-DEC-170` (4, 6), `192` (f) | — | API 3.7, 3.8 | `BE-RWI-148` | — | Akun "Cashier" tanpa `BillingInpatient : Read` → 403 (`AC-RWF-022`) | Belum dikerjakan |
| `FR-RWF-025` Tagihan belum terbentuk tampil apa adanya | `RWI-DEC-108` | — | API 3.7 (`NOT_FORMED`) | `BE-RWI-156` | `keperawatan` `FE-RWI-185` | `InvoiceState = NOT_FORMED` (`AC-RWF-023`) | Belum dikerjakan |
| `RWI-AC-330` Biaya operasi kunjungan asal tampil di Tagihan Pasien | `RWI-DEC-207` | Backend 9.14 | API 3.11; integrasi 4.6; data 6.9 | `BE-RWI-159` | `keperawatan` `FE-RWI-186` | Satu tautan, tidak ada baris berpindah (`UAT-RWF-39`, `40`); bagian "satu kwitansi" lihat gap G-27 | Belum dikerjakan |
| `INV-RWF-05` Pesan tidak membawa field di luar daftar putih | `RWI-DEC-166`, `207` | Integrasi 4.2 | Integrasi 4.6 | `BE-RWI-151`, `BE-RWI-159` | — | Isi pesan `ADMISSION_CONFIRMED` tanpa `SourceEncounterId` | Belum dikerjakan |
| Regresi rawat jalan | `RWI-DEC-153` (pola) | — | — | `BE-RWI-155` | — | Invoice rawat jalan identik sebelum/sesudah | Belum dikerjakan |

## 2. Definition of Done PRD → bukti

| Butir DoD (`04-prd-to-mvp.md` 8.19) | Task | Bukti |
|---|---|---|
| Tidak ada endpoint pengubah status kepulangan tanpa login | `BE-RWI-146` | `UAT-RWF-02`; verifikasi API `AC-RWF-001` |
| Status kasir hanya satu sumber | `BE-RWI-152`, `BE-RWI-153` | Pencarian kode `AC-RWF-005` |
| Invoice `RANAP` terbentuk otomatis dan tepat satu | `BE-RWI-150` | `UAT-RWF-01`, `UAT-RWF-11`; `INV-RWF-06` |
| Pesan outbox `Published` hanya dengan tanda terima | `BE-RWI-151` | `AC-RWF-016` |
| Tidak ada angka tarif atau jam potong tertanam | `BE-RWI-147` | Pencarian kode `AC-RWF-014` |
| Koreksi penempatan tidak menggandakan tarif kamar | `BE-RWI-154` | Dua uji wajib `IsSuperseded` |
| Rupiah tidak sampai ke perawat; hak rupiah bukan nama peran | `BE-RWI-148`, `BE-RWI-156` | `AC-RWF-020`, `021`, `022` |
| Episode aktif saat rilis punya invoice | `BE-RWI-157` + langkah operasional `I5` | `UAT-RWF-15` di lingkungan uji; `I5` produksi ⛔ menunggu wewenang tertulis |
| Regresi rawat jalan nol | `BE-RWI-155` | Kriteria 6 kartu `BE-RWI-155` |
| Master kebijakan tarif kamar dan tarif kelas terisi di lingkungan uji | Bukan task kode | Prasyarat lingkungan; diperiksa saat verifikasi `BE-RWI-150` dan `BE-RWI-155` (`02-backend-architecture.md` 9.11) |

## 3. Gap dan dependency luar

| ID | Gap | Dampak | Penanganan | Pemilik |
|---|---|---|---|---|
| G-27 | Penyelesaian multi-invoice dalam satu transaksi (`BKC-DEC-118` / `BILL-INT-007`) belum ada di source Billing | Bagian "satu transaksi, satu kwitansi" `RWI-AC-330` belum tercakup task mana pun di modul ini | Dependency luar; dikerjakan roadmap `billing-kasir`. `BE-RWI-159` mencakup tautan dan tampilan Tagihan Pasien saja | Yasmina |
| TRC-RWF-01 | Layar Kelayakan Keuangan `FE-INP-08` tidak disebut `03-frontend-architecture.md` bagian 6, padahal `POST financial-clearance` yang dipakainya dihapus API 3.1 | Tanpa task, formulir di layar itu gagal 404 setelah rilis | Dicakup `FE-RWI-166` butir 4 sebagai akibat langsung kontrak; bentuk layar riwayat `DEV_DISCRETION`. Kontrak frontend sebaiknya menyebutnya pada revisi berikutnya | Muhammad Hamzah |
| — | Langkah operasional `I5` (putar ulang produksi) | Episode aktif saat rilis belum punya invoice sampai `I5` dijalankan | ⛔ menunggu wewenang tertulis (`RWI-DEC-169` butir 5); bukan task builder | Muhammad Hamzah |

Tidak adanya automated test backend **bukan** gap (`rules/backend/TEST_POLICY.md`).
