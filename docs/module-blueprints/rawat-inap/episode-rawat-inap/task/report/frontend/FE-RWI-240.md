# Laporan Perubahan Frontend — `FE-RWI-240`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-240` |
| Judul | Kelas otomatis setelah memilih kamar pada admisi |
| Slice | PLAN-REPAIR-DOK-006 — perbaikan kelas perawatan dan cakupan asuransi per item |
| Roadmap | [frontend-roadmap-kelas-perawatan.md](../../../roadmap/frontend-roadmap-kelas-perawatan.md), kartu `FE-RWI-240` |
| Trace | FIX-DOK-006-06; ISS-DOK-006-02; keputusan A-05 dan K-01; aturan kontrak nomor 1 dan 4; [plan repair](../../../../dokter-rawat-inap/docs/plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md) bagian 4 |
| Contract version | [RWI-KELAS-006-1.0](../../../../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), disetujui pemilik 2026-10-12; SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112` |
| Wewenang UI | Menghapus pemilih kelas pada pembayaran dan konfirmasi; teks informasi memakai `InformationAlert` existing. Tidak ada komponen visual baru |
| Dependency | ✅ [BE-RWI-224](../backend/BE-RWI-224.md) — `OpenAdmissionRequest.SelectedBedId`, kelas dibaca server dari kamar, kelas konfirmasi diturunkan dari reservasi |
| Klasifikasi | `MEDIUM` — beberapa langkah admisi, payload API `POST /episodes` bertambah satu field opsional, tanpa route/state global baru |
| Task mode | `CROSS-REPO` — frontend target tulis; backend hanya laporan dan register |
| Target tulis | `QuilvianSystemFrontendDev` berkas admisi rawat inap pada tabel 3.2; laporan ini dan register `episode-rawat-inap` |
| Model | Implementasi: Codex berbasis GPT-6. Penutupan register: Claude Opus 5.5 |
| Commit frontend saat dikerjakan | Basis `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`; source di-commit pemilik pada `2d0a6890e`; HEAD saat penutupan `f4495aa14` (merge IGD/Finance, tidak menyentuh berkas task) |
| Commit backend yang dijadikan rujukan | Basis `d908988248f6652ed7f2ce84aae9eb3e38dd36a0`; HEAD saat penutupan `4ec02370` |
| Tanggal | 2026-10-13 |
| Status | ✅ SELESAI — seluruh acceptance criteria terpetakan ke source; lint, unit, dan build dijalankan; uji manual NOT FEASIBLE |

---

## 1. Keadaan yang ditemukan di awal

Langkah Pembayaran mewajibkan petugas memilih kelas perawatan sebelum tombol Lanjut aktif, dan langkah Konfirmasi juga memiliki pemilih kelas manual. Kelas itu disimpan di storage pembayaran lalu dipakai untuk membuka kunjungan, episode, kebijakan deposit, dan admisi transfer, terlepas dari kamar yang akhirnya dipilih. Akibatnya kelas pasien bisa berbeda dari kelas kamar sebenarnya.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi rawat inap.

1. **Pembayaran.** Petugas memilih cara bayar dan kartu penjamin. Tidak ada isian kelas. Layar menampilkan informasi "Kelas perawatan otomatis mengikuti kamar yang dipilih pada langkah Pilih Tempat Tidur." Tombol Lanjut hanya menunggu cara bayar, kartu penjamin, dan draft kartu yang belum disimpan.
2. **Dokter.** Petugas mengisi DPJP dan unit. Data hanya divalidasi di klien; belum ada kunjungan maupun episode yang dibuat.
3. **Pilih tempat tidur.** Petugas memilih bed lalu menekan "Lanjut ke Pemesanan". Tombol berubah menjadi "Menyimpan admisi..." dan sistem:
   - memeriksa bed punya kelas dan berada pada unit admisi. Contoh: kamar Melati kelas 1 pada unit Rawat Inap Lantai 2 diterima; kamar tanpa kelas ditolak dengan pesan "Kamar yang dipilih belum memiliki kelas perawatan. Pilih kamar yang sudah dikonfigurasi.";
   - membuat kunjungan dengan kelas kamar, lalu membuka episode dengan `selectedBedId` sehingga server membaca ulang kelas dan unit kamar;
   - menulis ID episode ke alamat halaman agar draft dapat dipulihkan bila halaman dimuat ulang.
4. **Pemesanan.** Booking bed berjalan seperti sebelumnya; konflik bed tetap diperiksa server.
5. **Konfirmasi.** Kelas tampil sebagai informasi dari episode, tidak dapat diedit. Yang masih dapat diubah hanya unit layanan dan catatan.
6. **Deposit.** Bila kelas belum ada, layar menampilkan "Kelas kamar belum ditetapkan — Kebijakan deposit menurut kelas dapat diperiksa setelah kamar dipilih. Nominal tetap dapat dicatat dan tidak menahan admisi." Setelah episode ada, kebijakan dibaca memakai kelas episode dari server.
7. **Admisi transfer dari IGD/Poli.** Kelas diambil dari bed tujuan saja, tidak lagi dari pilihan pembayaran.

Jalur tidak normal: klik ganda saat menyimpan ditahan oleh penanda `prepareInFlight`; bila pembukaan gagal, pesan "Admisi belum dapat disimpan. Periksa data kamar dan isian Dokter sebelum melanjutkan." tampil dan petugas dapat mencoba lagi; percobaan ulang setelah kunjungan berhasil tidak membuat kunjungan kedua karena `submit` mengembalikan data yang sudah tersimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend dan backend; `rules/frontend/REPORT_TEMPLATE.md`; skill `build-module-frontend`; kontrak RWI-KELAS-006-1.0; plan dan issue 006; laporan BE-RWI-224; seluruh berkas pada tabel 3.2 beserta diff `affcfcb4e..HEAD`; `InpEpisodeService.cs` dan `InpBedOccupancyService.cs` sebagai kontrak backend aktual.

### 3.2 Berkas yang berubah

Semua jalur relatif terhadap repository `QuilvianSystemFrontendDev`.

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx` | Hapus komponen pemilih kelas; ganti dengan informasi kelas mengikuti kamar |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-payment.jsx` | Hapus state, pilihan, dan penyimpanan kelas pembayaran |
| `src/utils/health-services/inpatient-management/inpatient-admission-payment-utils.jsx` | `validateInpatientPaymentSelection` tidak lagi mensyaratkan kelas |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | `submit({ selectedBed })` membuat kunjungan/episode dengan kelas kamar dan `selectedBedId`; idempoten setelah tersimpan |
| `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` | `resolveInpatientAdmissionRoom` menolak bed tanpa kelas atau beda unit; payload episode membawa `selectedBedId`; validasi penjamin tanpa kelas |
| `src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx` | Teks penjelasan penyimpanan setelah kamar dipilih |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | Konteks `admission` dan `prepareBooking` satu-jalan; status `preparing` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` | Tombol menunggu `prepareBooking` sebelum pindah ke Pemesanan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Penyimpanan ditunda sampai bed; ID episode ditulis ke URL; deposit memakai kelas episode; transfer memakai kelas bed |
| `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` | Hapus pemilih kelas; teks "Kelas perawatan mengikuti kamar yang dipilih" |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-confirmation.jsx` | Hanya unit/catatan yang dapat diubah; payload memakai kelas episode |
| `src/utils/health-services/inpatient-management/inpatient-admission-confirmation-utils.jsx` | Pesan kelas kamar belum tersedia; kelas tidak dihitung sebagai perubahan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx` | Informasi "Kelas kamar belum ditetapkan" |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx` | Tidak memanggil kebijakan deposit tanpa kelas; `awaitingRoomClass` |
| `src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js` | Kelas transfer hanya dari bed tujuan |
| `tests/unit/inpatient-class-coverage.test.mjs` | Tes 3–5: pembayaran tanpa kelas, kelas dari kamar, konfirmasi tanpa kelas manual |

Perubahan lain pada commit `2d0a6890e` (Workspace PPRI, relokasi menu Transfer, bed transfer, kasir) bukan bagian task ini.

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → hook → service existing; tidak ada Redux slice, service, atau route baru. Elemen yang dipakai:

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Penjamin | `REUSE` `BasePayerWorkspace` | Hanya sisipan kelas pada pemanggil yang dihapus |
| Bed, stepper, ringkasan | `REUSE` komponen admisi existing | Kelas berasal dari bed |
| Pesan informasi/gagal | `REUSE` `InformationAlert` | Varian `info` dan `danger` existing |
| Tombol | `REUSE` `BaseButton` | Label sibuk dan guard klik ganda |

UI GATE: seluruh elemen `REUSE`. Grep anti-regresi pada baris tambahan: tidak ada warna/ukuran literal, gaya inline, atau kontrol HTML mentah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menyimpan admisi..." pada tombol bed; indikator kebijakan deposit existing |
| Kosong | Kelas belum ada sebelum bed dipilih: informasi pada Pembayaran dan Deposit, bukan pilihan kelas palsu |
| Gagal | Pesan kamar tanpa kelas, unit berbeda, atau "Admisi belum dapat disimpan..."; petugas dapat memilih ulang bed dan mencoba lagi |
| Tanpa hak akses | Tidak berubah; permission `InpatientEpisode`, `PatientEncounter`, `InpatientBedOccupancy`, `BillingDeposit` existing |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/admin` | Membuat kunjungan rawat inap dengan kelas kamar | `PatientEncounter : Create` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/episodes` | Membuka episode; field opsional baru `selectedBedId` | `InpatientEpisode : Create` |
| `PUT` | `/api/v1/health-services/inpatient-management/episodes/{id}` | Konfirmasi unit/catatan; kelas diturunkan server | `InpatientEpisode : Update` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Booking bed; server menyelaraskan kelas draft dan menolak kamar tanpa kelas (422) | `InpatientBedOccupancy : Create` |

#### Health Services / Billing Management / Billing / Patient Funds

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` | Kebijakan deposit menurut penjamin dan kelas episode | `BillingDeposit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-class-coverage.test.mjs` | 5/5 lulus (3 milik task ini) | `PASS` | Dijalankan ulang pada sesi penutupan 13 Oktober 2026 |
| `npx eslint --quiet` pada 17 berkas FE repair `FE-RWI-239`/`FE-RWI-240` | Exit 0, tanpa error | `PASS` | Sesi penutupan; log `%TEMP%/quilvian-kelas-fe-eslint-closure.log` |
| Unit 14 berkas terkait admisi dan resep, termasuk `inpatient-admission-deposit`, `-transfer`, `-consent`, `-payment-storage`, `inpatient-bed-admission-helper`, `inpatient-transfer-repair-006` | 92 tes: 87 lulus, 5 gagal | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `FE-RWI-058` menguji `minimumAmount` yang sudah ada di basis `affcfcb4e`; empat lainnya menguji berkas yang tidak berubah sejak basis. Log `%TEMP%/quilvian-kelas-fe-unit-closure.log` |
| `inpatient-admission-workspace.test.mjs` dan `inpatient-transfer-billing-relocation.test.mjs` | 21 tes: 16 lulus, 5 gagal | `EXISTING / ENVIRONMENT ISSUE` | Gagal karena menu Transfer dihapus dari Workspace PPRI (`admission-workspace-sections.jsx`, konstanta workspace). Itu pekerjaan relokasi PPRI, bukan task ini |
| Pengujian terarah sesi implementasi | 39/39 lulus | `PASS` | 13 Oktober 2026, sebelum commit `2d0a6890e` |
| `node --max-old-space-size=8192 ./node_modules/next/dist/bin/next build` lalu `prepare-standalone` | Berhasil | `PASS` | Sesi implementasi 13 Oktober 2026; source task tidak berubah sesudahnya |
| Suite global | 2776 tes: 2742 lulus, 34 gagal; snapshot HEAD pembanding 2761/2732/29 | `EXISTING / ENVIRONMENT ISSUE` | Selisih lima kegagalan = dua berkas PPRI di atas |

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-class-coverage.test.mjs` — PASS (5/5).

Uji manual: `NOT FEASIBLE` — sesi tidak memiliki browser/aplikasi aktif, dan aplikasi yang berjalan belum memuat build baru. Alur pembayaran → bed → konfirmasi → deposit belum dibuktikan di layar.

**Tidak dijalankan:** build produksi ulang pada sesi penutupan, karena source task tidak berubah sejak build sesi implementasi dan merge `f4495aa14` tidak menyentuh berkas task.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Hapus field/progress/validasi kelas pada pembayaran | Terpenuhi | `ServiceClassField`/`ServiceClassSection` dihapus; validasi tanpa kelas; tes 3 |
| Draft dokter ditunda sampai pilih kamar, lalu kunjungan dan episode memakai kelas kamar | Terpenuhi | `deferCreation: true`; `submit({ selectedBed })`; `resolveInpatientAdmissionRoom`; `selectedBedId` pada payload; tes 4 |
| Deposit dan transfer tidak membaca kelas dari storage pembayaran | Terpenuhi | Deposit memakai `resume.patientClassId`; transfer memakai `bed.patientClassId` saja |
| Validasi penjamin, isolasi, konflik bed, dan retry tetap berlaku | Terpenuhi | Validasi kartu tetap (tes 3); booking memakai operasi existing; retry idempoten |
| Konfirmasi tanpa kelas manual | Terpenuhi | Pemilih kelas dihapus; `handleChange` hanya unit/catatan; tes 5 |
| Contoh kamar kelas 1/item dijamin kelas 2 tanpa menyembunyikan item | Terpenuhi | Kelas episode = kelas kamar; cakupan item mengikuti BE-RWI-223/225/226 |
| DoD: source, validasi, laporan tracked, traceability | Terpenuhi | Bagian 6; [roadmap](../../../roadmap/frontend-roadmap-kelas-perawatan.md) dan [traceability](../../../roadmap/requirement-traceability-kelas-perawatan.md) |
| DoD: uji manual bila runtime tersedia | `NOT FEASIBLE` | Runtime tidak tersedia; tidak diklaim lulus |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kegagalan unit lama pada berkas terkait; tidak disebabkan task ini |
| Masalah yang diketahui | Uji manual alur lengkap belum dilakukan. Kelas yang tampil sebelum penempatan adalah kelas kamar pilihan, bukan kelas aktual layanan; kelas aktual baru sah setelah placement (BE-RWI-224) |
| Dependency backend | Tidak ada yang tertunda; ✅ BE-RWI-224 |
| Perubahan sampingan | `NONE` |
| Interupsi | Laporan ditulis pada sesi penutupan terpisah; source tidak dikerjakan ulang |
| Status Git | Frontend bersih pada `f4495aa14`; source task sudah di-commit pemilik pada `2d0a6890e`. Laporan ini belum di-commit di backend |
| Langkah berikutnya | Uji manual admisi baru: pembayaran asuransi tanpa kelas → pilih kamar kelas 1 → konfirmasi → deposit, setelah aplikasi menjalankan build terbaru |
