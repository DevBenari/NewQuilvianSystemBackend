# Laporan Live Browser Testing — Layar Pengembangan & Administrasi (Gelombang 4, `FE-HRD-R46`)

| Item | Keterangan |
| --- | --- |
| Menu yang diuji | **Pendidikan & Pelatihan RS** (`S-C2`, 8 rute), **Manajemen Kinerja & Karier** (`S-C3`, 8), **Manajemen Manfaat** (`S-D3`, 11), **Onboarding & Kontrak** (`S-C4-ONB`, 9), **Cuti & Perizinan** (`S-B2`, 7), **Penjadwalan & Roster** (`S-B4`, 3), **Pengadaan & Rekrutmen** (`S-D2`, 7), **Potongan & Restitusi** (`S-B6`, 5), **layar lama/ESS** (8) — total 66 rute |
| Task roadmap | `FE-HRD-R46` — Verifikasi layar pengembangan dan administrasi |
| Tanggal uji | Rabu, 18 November 2026, pagi hingga 11.45 WIB |
| Penguji | AI Agent (Claude Code, skill `live-browser-testing`) dengan Google Chrome (Chromium) mode visual/headed |
| Lingkungan | Frontend `http://localhost:3000` (branch `HamzahV2`; sapu awal memakai build produksi, verifikasi perbaikan memakai `next dev`); backend `https://localhost:7184` (branch `MHamzah`, build scratch, `Runtime__Role=Web`) |
| Database | `QuilvianNewDevHamzah` |
| Akun | `superadmin@admin.com` (admin, tanpa profil pegawai), `dewi.anggraeni@rsmmc.local` (pegawai non-admin), `mira.safitri@rsmmc.local` (pegawai, untuk Slip Gaji) |
| Persetujuan pemilik | 18 Nov 2026: uji membaca, galat disimulasikan di browser, tanpa penulisan data dari menu ini. Tombol simpan pada uji validasi tidak pernah ditekan. |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: regresi frontend yang ditemukan sudah diperbaiki dan terbukti di browser, tetapi dua layar Kinerja masih gagal karena backend.** Acceptance criteria "Tidak ada regresi fungsi" belum terpenuhi penuh, sehingga `FE-HRD-R46` berstatus 🟡.

Contoh di rumah sakit:
- Sebelum perbaikan, perawat yang membuka **Portofolio SKP** melihat layar galat karena satu baris kode liar.
- Layar **Slip Gaji** menampilkan slip contoh atas nama "Ns. Siti Rahmawati" kepada siapa pun.
- Bila server gagal, PIN `123456` membuka rincian gaji contoh tersebut.

Ketiganya sudah diperbaiki. Sementara itu, **Penilaian Kinerja Saya** dan **Keberatan Penilaian** masih HTTP 500 untuk semua pengguna karena bug `GET my-appraisal` di backend.

| Ringkasan | Jumlah |
| --- | ---: |
| Test case | 157 (66 admin + 66 non-admin + 10 simulasi galat + 6 validasi formulir + 9 Slip Gaji) |
| ✅ Lulus | 149 (termasuk 1 lulus dengan catatan) |
| ❌ Gagal → ✅ diperbaiki dan diverifikasi ulang | 6 (Portofolio SKP admin & non-admin; pesan Axios di ESS Lembur admin & non-admin dan Koreksi Presensi non-admin; Slip Gaji data contoh) |
| ❌ Gagal, blocker backend | 2 (Penilaian Saya, Keberatan Penilaian) |
| ⚠️ Temuan terbuka lain | 4 (LBT-W4-PA-04, -05, -07, -08) |

**Yang sudah berjalan baik:**
- Simulasi `500`/`403` pada Rekonsiliasi Cuti, Roster, Kandidat, Persetujuan Restitusi, dan Sesi Diklat: 10/10 pesan ramah, tanpa data contoh.
- Validasi formulir: NIK kandidat 5 digit ditolak ("NIK harus 16 digit angka (VR-REC-05)."). Draf penyesuaian cuti dan periode roster kosong tidak dapat disimpan. Dialog tutup periode cuti terbuka dan dapat dibatalkan.
- Batas potongan restitusi 5% otomatis dikoreksi ke batas bawah 10%, sehingga nilai tidak sah tidak bisa dimasukkan.

---

## 2. Alur Proses Bisnis yang Diuji

1. **Sapu layar** — 66 rute dibuka oleh admin dan pegawai non-admin. Agent mencatat HTTP 5xx, crash JavaScript, layar kosong, pesan Axios mentah, dan pesan tanpa hak.
2. **Kegagalan server** — lima layar contoh dijawab tiruan `500` dan `403`.
3. **Validasi formulir (tanpa menyimpan)**:
   - Daftarkan kandidat dengan NIK 5 digit.
   - Kebijakan restitusi dengan batas potongan 5%.
   - Draf penyesuaian cuti kosong.
   - Periode roster kosong.
   - Tab voucher kasir.
   - Dialog tutup periode cuti.
4. **Slip Gaji ESS** (setelah disambungkan ke API pada sesi ini):
   1. Pegawai membuka **Slip Gaji Saya**; daftar dibaca dari API nyata.
   2. Pegawai menekan **Lihat Rincian**, lalu papan PIN muncul.
   3. Pegawai memasukkan 6 digit dan menekan **Buka Slip Gaji**.
   4. Server memverifikasi PIN dan mengembalikan token sesi 15 menit.
   5. Rincian dibaca dengan header `X-Payroll-PIN`.
   6. **Unduh PDF** memakai token yang sama.
   7. Bila sesi PIN habis, layar kembali terkunci dengan pesan server.

   Verifikasi PIN asli menulis sesi/lockout ke DB, sehingga langkah 3–7 dijalankan dengan jawaban server tiruan di browser.

## 3. Persiapan Lingkungan & Data

Sama dengan [laporan R42](laporan-live-testing-gelombang4-perencanaan-tenaga-kerja.md#3-persiapan-lingkungan--data). Pemeriksaan baca-saja DB: tabel `PrlPayrollPayslip` berisi **0 baris**, sehingga daftar slip kosong untuk semua akun.

## 4. Endpoint yang Diuji

Auth: cookie sesi JWT + `[AccessPermission]`.

### `[Tags("HR Payroll - Self-Service Payslips")]` — `PayrollRunModernController`

| Method | Path | Deskripsi | Request | Respons yang teramati |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/corporate/human-resource/payroll/runs/self-service/my-payslips` | Daftar slip terbit milik pegawai | query `year` (opsional) | API nyata `200`, `data: []` (akun Mira) |
| POST | `/api/v1/corporate/human-resource/payroll/runs/self-service/verify-pin` | Verifikasi PIN 6 digit; 3× salah = kunci 30 menit | `{ "pin": "150895" }` | Tiruan: `200` + `pinToken`, `400` "Sisa kesempatan: 2 kali", `429` "terkunci selama 30 menit" |
| GET | `/api/v1/corporate/human-resource/payroll/runs/self-service/payslips/{payslipId}/detail` | Rincian penerimaan/potongan | header `X-Payroll-PIN: <token>` | Tiruan `200`; header terkirim ✅ |
| GET | `/api/v1/corporate/human-resource/payroll/runs/self-service/payslips/{payslipId}/download-pdf` | PDF slip | header `X-Payroll-PIN: <token>` | Tiruan `401` (sesi habis) → layar terkunci kembali; header terkirim ✅ |

### `[Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/hrd/performance/appraisals/my-appraisal` | Lembar penilaian diri | **`500`** untuk admin (lihat LBT-W4-PA-01) |

### `[Tags("Self Services / Human Resource / Overtime")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/self-services/human-resource/overtime/...` | Data lembur mandiri | `404` "Workforce profile untuk akun login tidak ditemukan atau tidak aktif." Setelah perbaikan, pesan ini tampil apa adanya (sebelumnya "Request failed with status code 404") |

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate")]`, `[Tags("HR Leave - Reconciliations & Close")]`, `[Tags("HR Scheduling - Roster Period")]`, `[Tags("HR Deductions - Restitution Claims")]`, `[Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/human-resource/recruitment/candidates` | Bank pelamar | `200`; simulasi → pesan ramah |
| GET | `/api/v1/corporate/human-resource/leave/...` (rekonsiliasi) | Rekonsiliasi & tutup periode cuti | `200`; simulasi → pesan ramah |
| GET | `/api/corporate/human-resource/scheduling/roster-periods` | Periode roster | `200`; simulasi → pesan ramah |
| GET | `/api/hr-deductions/...` | Antrean klaim restitusi & voucher | `200`; simulasi → pesan ramah |
| GET | `/api/v1/corporate/hrd/learning/sessions` | Sesi diklat | `200`; simulasi → pesan ramah |

## 5. Rincian Test Case

### P0 — Critical / Fatal Functional

| ID | Skenario | Hasil |
| --- | --- | --- |
| PA-P0-01 | Portofolio SKP (`/corporate/hrd/learning/skp-tracker`) dibuka | ❌ crash JS (baris liar `q;` di `page.jsx`, berasal dari commit `f38b84d7f`) → ✅ diperbaiki; bukti `verifikasi-perbaikan/01-skp-tracker.png` |
| PA-P0-02 | Penilaian Kinerja (`/corporate/hrd/performance/appraisals`) dibuka admin | ❌ HTTP 500 dari `my-appraisal` (LBT-W4-PA-01) |
| PA-P0-03 | Keberatan Penilaian (`/corporate/hrd/performance/disputes`) dibuka admin | ❌ HTTP 500 dari `my-appraisal` (LBT-W4-PA-01) |
| PA-P0-04 | 63 layar lain (admin) dan 66 layar (non-admin) | ✅ tanpa 5xx/crash/blank; 38 layar non-admin menolak dengan pesan tanpa hak |

### P1 — Business Compliance

| ID | Skenario | Hasil |
| --- | --- | --- |
| PA-P1-01 | Slip Gaji tidak lagi menampilkan data contoh | ❌ → ✅ data dari API; nomor slip contoh `SLIP/2026/11/042` dan nama contoh hilang (`R46-slip-gaji/01-api-nyata.png`) |
| PA-P1-02 | Nominal disamarkan sampai PIN terverifikasi | ✅ "Rp ••••••" di tabel dan kartu ringkasan (`02-daftar-tersamar.png`) |
| PA-P1-03 | PIN demo `123456` saat server `500` | ✅ ditolak, rincian tidak terbuka (`07-pin-demo-123456-server-500.png`) |
| PA-P1-04 | PIN demo `251126` saat koneksi putus | ✅ ditolak (`07-pin-demo-251126-server-koneksi-putus.png`) |
| PA-P1-05 | PIN salah → pesan server "Sisa kesempatan: 2 kali" | ✅ (`03-pin-salah.png`) |
| PA-P1-06 | `429` → papan terkunci, hitung mundur tampil | ✅ (`04-terkunci.png`) |
| PA-P1-07 | PIN benar → rincian terbuka; header `X-Payroll-PIN` terkirim | ✅ (`05-rincian-terbuka.png`) |
| PA-P1-08 | Unduh saat sesi PIN habis (`401`) → terkunci kembali, tetap login | ✅ (`06-sesi-pin-berakhir.png`) |
| PA-P1-09 | Simulasi `500`/`403` pada 5 layar | ✅ 10/10 pesan ramah |
| PA-P1-10 | Pesan galat ESS Lembur/Koreksi Presensi/Cuti/Jadwal tidak memuat teks Axios | ❌ → ✅ diperbaiki (`verifikasi-perbaikan/02` s.d. `05`) |

### P2 & P3 — Functional & Validation

| ID | Skenario | Hasil |
| --- | --- | --- |
| PA-P2-01 | Kandidat dengan NIK `12345` | ✅ pesan VR-REC-05, tombol **Daftarkan** nonaktif |
| PA-P2-02 | Batas potongan restitusi `5` | ✅ dengan catatan: kolom angka mengoreksi otomatis ke `10` (batas bawah), sehingga pesan galat tidak pernah muncul |
| PA-P2-03 | Draf penyesuaian cuti kosong | ✅ **Simpan Draf** nonaktif; alasan menunggu saldo dipilih |
| PA-P2-04 | Periode roster kosong | ✅ **Simpan & Buka Kisi** nonaktif |
| PA-P2-05 | Tab **Voucher Kasir** di Persetujuan Restitusi | ✅ tersedia (kosong) |
| PA-P2-06 | Dialog **Tutup & Kunci Periode** cuti | ✅ terbuka dan dibatalkan tanpa menutup periode |
| PA-P2-07 | Papan PIN: tombol **Buka Slip Gaji** nonaktif sampai 6 digit; 5 digit tidak terkirim otomatis | ✅ |

### P4 — Visual & P5 — UI/UX

Layar Slip Gaji ditulis ulang dengan Hero, SummaryGrid, DataTable, StatusBadge, panel rincian, dan `ConfirmModal`, tanpa warna literal. Screenshot `05-rincian-terbuka.png` dan `03-pin-salah.png` menunjukkan tampilan seragam dengan layar HR lain.

## 6. Daftar Temuan & Rekomendasi

### P0 — Fatal

| ID | Temuan | Contoh dampak | Rekomendasi |
| --- | --- | --- | --- |
| LBT-W4-PA-01 | `GET appraisals/my-appraisal` mengambil `workforceProfileId` dan `cycleId` dari **query string**, bukan dari akun login. Frontend tidak mengirimnya, sehingga keduanya menjadi `Guid.Empty`. Service lalu membuat `HrdPerformanceAppraisal` baru di dalam GET (`SaveChangesAsync`) dan gagal FK, sehingga HTTP 500 untuk semua pengguna. | Perawat tidak bisa membuka lembar penilaian dirinya. Selain itu, pengguna yang berhak `ViewOwn` bisa mengisi ID pegawai lain di query dan membuka atau membuat penilaian orang lain (celah akses). | Backend: tentukan profil dari akun login, seperti `OvertimeSelfServiceContextService` (email → `WorkforceProfile`), dan tentukan siklus aktif di server. Jangan membuat data di dalam GET; pindahkan ke aksi POST eksplisit. |
| LBT-W4-PA-02 | Portofolio SKP crash (`q is not defined`) | Seluruh layar tidak dapat dibuka | ✅ Diperbaiki |

### P1 — Kepatuhan bisnis

| ID | Temuan | Contoh dampak | Rekomendasi |
| --- | --- | --- | --- |
| LBT-W4-PA-03 | Slip Gaji memakai data contoh tetap, dan modal PIN menerima `123456`/`251126` saat API gagal | Pegawai melihat gaji orang lain (contoh) dan mengira itu slipnya | ✅ Diperbaiki: API nyata, PIN hanya diputus server, token di memori |
| LBT-W4-PA-04 | `PayrollRunModernController` memakai `ClaimTypes.NameIdentifier` (ID akun login) sebagai kunci pegawai, lalu mencocokkannya dengan `WorkforceProfileId`/`MstEmployee.Id`. Untuk akun Mira, ID akun ≠ ID profil ≠ ID pegawai (diperiksa baca-saja). | Setelah slip diterbitkan, daftar slip pegawai tetap kosong, dan setiap PIN dianggap salah sampai akun terkunci 30 menit | Backend: petakan akun ke profil pegawai (email → `WorkforceProfile`) sebelum query slip, verifikasi PIN, rincian, dan unduhan |
| LBT-W4-PA-05 | Menu Diklat (8/8), Kinerja (8/8), Manfaat (9/11), Onboarding (9/9), Cuti (6/7) terlihat oleh pegawai biasa walau sebagian halaman menolak | Navigasi membingungkan | Frontend: tambahkan `requiredPermission` yang sesuai |

### P2/P3 — Fungsional

| ID | Temuan | Rekomendasi |
| --- | --- | --- |
| LBT-W4-PA-06 | Pesan Axios mentah di empat layar ESS lama | ✅ Diperbaiki memakai `getHrApiErrorMessage` |
| LBT-W4-PA-07 | Rute lama `/self-services/human-resource/overtime` tidak dirujuk menu maupun portal ESS (portal menunjuk `/hr/self-service/overtime`) | Pemilik menu memutuskan: arahkan ke rute baru atau hapus |
| LBT-W4-PA-08 | `GeneratePayslipPdfDocument` menyusun PDF mentah tanpa enkripsi, sehingga PDF slip **tidak berkata sandi** (VR-PAY-24 belum dipenuhi). Layar lama menjanjikan "Password PDF: tanggal lahir DDMMYYYY". | Backend: pasang proteksi sandi PDF sesuai VR-PAY-24. Frontend sudah menghapus petunjuk sandi yang keliru; petunjuk dikembalikan setelah backend memasang sandi. |

## 7. Tidak Dapat Diuji / Tertahan

| Hal | Alasan |
| --- | --- |
| Slip Gaji dengan data nyata (PIN asli, rincian, unduhan PDF) | `PrlPayrollPayslip` kosong; verifikasi PIN menulis DB; LBT-W4-PA-04 |
| Simpan data pada formulir yang divalidasi | Di luar izin penulisan Gelombang 4 |
| Penilaian Kinerja dan Keberatan | ⛔ LBT-W4-PA-01 |

## 8. Data Uji yang Tertinggal di `QuilvianNewDevHamzah`

Tidak ada. Pemeriksaan baca-saja 18 Nov 2026 pukul 11.40 WIB: `HrdPayslipPinSession` dan `HrdPayslipPinLockout` 24 jam terakhir = 0 baris.

## 9. Lampiran Teknis

| Berkas | Isi |
| --- | --- |
| `test-with-agy/scripts/hrw4-1118-r46-validation.mjs` | Enam uji validasi tanpa menyimpan |
| `test-with-agy/scripts/hrw4-1118-payslips.mjs`, `hrw4-1118-payslips-demo-pin.mjs` | Uji Slip Gaji (API nyata + tiruan) |
| `test-with-agy/scripts/hrw4-1118-verify-fixes.mjs` | Verifikasi ulang SKP, ESS lama, PSV |
| `test-with-agy/screenshots/hrw4-1118/R46-admin/`, `R46-nonadmin/`, `R46-kegagalan-server/`, `R46-validasi/`, `R46-slip-gaji/`, `verifikasi-perbaikan/` | Screenshot dan `hasil.jsonl` |

## 10. Tindak Lanjut

1. **Backend:** LBT-W4-PA-01 (`my-appraisal`), LBT-W4-PA-04 (pemetaan akun → pegawai di slip gaji), dan LBT-W4-PA-08 (sandi PDF slip).
2. Frontend: `requiredPermission` menu (LBT-W4-PA-05); keputusan rute lembur lama (LBT-W4-PA-07).
3. Setelah ada slip terbit di DB dev dan LBT-W4-PA-04 diperbaiki: uji Slip Gaji ujung ke ujung dengan PIN asli.
