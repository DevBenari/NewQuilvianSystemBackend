# Laporan Uji Tambahan DPJP Dokter Jaga IGD — R-16 (`FE-IGD-037` Acceptance 7)

Tanggal pelaksanaan: 2026-10-11  
Pelaksana: Antigravity (Pair Programming AI Assistant)  
Panduan acuan: `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md` Bagian 5  
Folder bukti mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-r16-dpjp-2026-10-11/`  
Dasar putusan: `IGD-DEC-263` (Uji layar roster kosong dengan daftar dokter tiruan) dan `IGD-DEC-260`…`264`

---

## 1. Metadata Lingkungan Pengujian

| Komponen | Nilai / Kondisi | Keterangan |
| --- | --- | --- |
| Tanggal Pengujian | 2026-10-11 | Putaran tambahan R-16 (Acceptance 7 FE-IGD-037) |
| Jam Selesai Putaran | 21:44 WIB | Selesai pada pemeriksaan akhir API langkah 4 |
| Server Frontend | Standalone Production Build (`node .next/standalone/server.js`) | Dilayani pada port 3000 dari build 11 Oktober 2026 20.51 WIB; terbukti `nextjs-portal === null` dan nol lencana "N" di seluruh tangkapan layar |
| Layanan Backend | `https://localhost:7184` | ASP.NET Core Kestrel (.NET 9) dari build pemilik |
| Akun Pengujian | `dimas.kurniawan@rsmmc.local` | Akun Perawat IGD dari `process.env.QUILVIAN_PERAWAT_EMAIL` (`isSuperAdmin: false`) |
| Viewport Browser | 1440 × 900 (DPR 1.0) | Playwright Chromium (Google Chrome), sidebar menu terbuka |
| Header Otorisasi & Sandi | Disamarkan (`[REDACTED]`) | Header `Authorization`, `Cookie`, dan payload login disamarkan |
| Modifikasi Data Master & SQL | Nol (0) | Nol perubahan jadwal/unit/role master, nol kueri SQL |
| Penulisan Data yang Terjadi | Tepat 1 `POST` | Tepat satu baris penetapan DPJP V3 pada Langkah 2 |

---

## 2. Pemeriksaan Awal (P0)

Pemeriksaan awal terhadap status kunjungan V3 sebelum penetapan R-16:

| Butir | Data Acuan | Hasil Pemeriksaan P0 | Evaluasi |
| --- | --- | --- | :---: |
| Emergency Visit ID | `63f12a1a-4a67-460b-8880-0a863937e87c` | Cocok | **PASS** |
| Encounter ID | `8051077d-162b-4ba0-9e3e-24c44aa2896e` | Cocok | **PASS** |
| Pasien di Layar | `ANISA PRAMESTI` | Terverifikasi pada header modal dan triage | **PASS** |
| Riwayat Penugasan Awal | Harus 0 baris | `GET ...?emergencyVisitId={V3}` menghasilkan status HTTP 200, array data panjang 0 | **PASS** |

Bukti tersimpan dalam rekaman jaringan dan log awal skrip.

---

## 3. Matriks Hasil Pengujian Skenario R-16

Intersepsi jaringan dipasang sebelum membuka Pintu A V3 persis sesuai panduan:
```javascript
await page.route('**/emergency-doctor-assignments/on-duty-doctors**', async (route) => {
  const response = await route.fetch();
  const json = await response.json();
  json.data = (json.data || []).filter((d) => d.isOnDuty === false);
  return route.fulfill({ response, json });
});
```

| Langkah | Deskripsi Langkah | Harapan Panduan | Hasil Pengamatan Riil | Bukti | Evaluasi |
| :---: | :--- | --- | --- | --- | :---: |
| **1** | Buka pintu A untuk V3, tekan *Tetapkan Dokter Sekarang*, buka pemilih dokter. | 1. Keterangan persis *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."* tampil.<br>2. Judul *Di luar jadwal jaga — wajib alasan* tampil.<br>3. Judul *Dokter jaga IGD saat ini* **tidak** tampil.<br>4. Tidak ada label opsi yang memuat *"Dokter Jaga IGD"*. | 1. Teks keterangan persis terdeteksi pada modal.<br>2. Judul *Di luar jadwal jaga — wajib alasan* tampil di baris atas opsi dropdown.<br>3. Judul *Dokter jaga IGD saat ini* tidak ada.<br>4. Label opsi hanya memuat *dr. Ranger Biru* dan *dr. Rendy Pangalila* (keduanya di luar jadwal jaga); nol kemunculan *"Dokter Jaga IGD"*. | `R-16-kosong.png`<br>`R-16-network.json` | **PASS** |
| **2** | Pilih D2 (*dr. Ranger Biru*), isi alasan *"Uji roster kosong"*, tekan *Tetapkan Dokter*. Tunggu respons `POST` dengan `page.waitForResponse`. | 1. `POST` membawa `rosterOverrideReason` *"Uji roster kosong"*.<br>2. Respons HTTP 201 dengan `isRosterOverride=true`. | 1. Payload `POST` terverifikasi mengirim `rosterOverrideReason: "Uji roster kosong"` ke `emergency-doctor-assignments`.<br>2. Backend membalas HTTP 201 dengan `isRosterOverride: true` dan `doctorName: "dr. Ranger Biru"`. | `R-16-network.json` | **PASS** |
| **3** | Tunggu 1500 ms, tangkap layar banner DPJP pintu A. | Banner menampilkan *dr. Ranger Biru* dan baris persis *"Di luar jadwal jaga — alasan: Uji roster kosong"*. | Banner hijau DPJP aktif menampilkan: `Dokter Penanggung Jawab IGD (DPJP): dr. Ranger Biru [AKTIF BERTUGAS]` dan baris kedua `Di luar jadwal jaga — alasan: Uji roster kosong`. Tombol *Alihkan Dokter* muncul di sebelah kanan. | `R-16-banner.png` | **PASS** |
| **4** | `GET …?emergencyVisitId={V3}` lewat API. | Satu baris D2, `isRosterOverride=true`, `rosterOverrideReason` *"Uji roster kosong"*. | HTTP 200; tepat 1 baris penugasan dengan `doctorId: "8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab"` (dr. Ranger Biru), `isRosterOverride: true`, dan `rosterOverrideReason: "Uji roster kosong"`. | `R-16-api.json` | **PASS** |

---

## 4. Daftar Seluruh Percobaan (`percobaan.json`)

Sesuai Aturan 8, 9, 10, dan 11, setiap sesi percobaan dicatat transparan di `percobaan.json` tanpa menghapus atau menimpa berkas bukti:

| No | Skrip | Tujuan / Skenario | Jam Mulai (WIB) | Jam Selesai (WIB) | Request | Hasil | Alasan / Catatan |
| ---: | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| 1 | `run-r16.mjs` | R-16: Roster kosong dengan daftar dokter tiruan | 21:42:09 | 21:42:09 | 0 | **SCRIPT ERROR** | Ekstraksi token login membaca `res.body.data.token` padahal token dikirim via `Set-Cookie` (`quilvian_access_token`). Dihentikan sebelum ada request ke V3. Diperbaiki pada `run-r16-p2.mjs`. |
| 2 | `run-r16-p2.mjs` | R-16: Roster kosong dengan daftar dokter tiruan | 21:43:40 | 21:43:40 | 0 | **SCRIPT ERROR** | `page.goto` timeout 30000ms pada `waitUntil: 'networkidle'` akibat aktivitas polling latar belakang. Dihentikan sebelum ada POST ke V3. Diperbaiki pada `run-r16-p3.mjs` dengan `domcontentloaded`. |
| 3 | `run-r16-p3.mjs` | R-16: Roster kosong dengan daftar dokter tiruan | 21:44:29 | 21:44:45 | 19 | **PASS** | Langkah 1 (keterangan & header di dropdown), Langkah 2 (POST 201 `isRosterOverride=true`), Langkah 3 (banner teks override), dan Langkah 4 (GET API riwayat 1 baris) seluruhnya terbukti terpenuhi persis. |

---

## 5. Pernyataan Kepatuhan Aturan yang Mengikat (Aturan 1–11)

1. **Aturan 1 (Build Produksi Port 3000)**: Terbukti dilayani oleh `node .next/standalone/server.js` (build 20.51 WIB). Terbukti `nextjs-portal === null` dan nol lencana "N" di pojok kiri bawah pada seluruh tangkapan layar (`R-16-kosong.png` dan `R-16-banner.png`).
2. **Aturan 2 (Dimensi Layar & Jeda Screenshot)**: Viewport 1440 × 900, menu samping terbuka, tangkapan layar diambil dengan jeda minimal 1000–1500 ms sesudah respons selesai dimuat.
3. **Aturan 3 (Akun Perawat IGD)**: Login memakai akun `dimas.kurniawan@rsmmc.local` dari variabel lingkungan (`isSuperAdmin: false`). Nol permintaan ke rute SuperAdmin.
4. **Aturan 4 (Kerahasiaan Kredensial)**: Nol sandi atau token di skrip, JSON bukti, maupun laporan. Header `Authorization`, `Cookie`, dan payload login disamarkan menjadi `[REDACTED]`.
5. **Aturan 5 (Integritas Data Master & Tanpa SQL)**: Nol perubahan terhadap jadwal dokter, unit layanan, atau role access. Nol kueri SQL dijalankan.
6. **Aturan 6 (Batasan Penulisan Data)**: Penulisan data hanya tepat satu kali `POST /emergency-doctor-assignments` untuk V3 pada Langkah 2. Nol penulisan data liar.
7. **Aturan 7 (Pencatatan Seluruh Permintaan)**: Setiap permintaan dicatat di `percobaan.json` dan rekaman jaringan `R-16-network.json`.
8. **Aturan 8 (Penyimpanan Seluruh Versi Skrip & URL Jaringan)**: Tiga versi skrip (`run-r16.mjs`, `run-r16-p2.mjs`, `run-r16-p3.mjs`) disimpan utuh di folder bukti. Rekaman jaringan mencakup seluruh panggilan `/api/v1/`.
9. **Aturan 9 (Kecocokan Log & Nama Pasien Riil)**: Log percobaan memuat jam mulai-selesai WIB yang cocok dengan aktivitas backend. Nama pasien diverifikasi langsung dari antarmuka layar sebagai **ANISA PRAMESTI**.
10. **Aturan 10 (Penggunaan `page.route`)**: `page.route` digunakan tepat sesuai panduan bagian 5 baris 135–140 untuk menyaring daftar dokter jaga agar menyimulasikan roster kosong.
11. **Aturan 11 (Distribusi Status & Tanpa Frasa Terlarang)**: Kegagalan skrip diklasifikasikan secara transparan sebagai `SCRIPT ERROR`. Laporan ini tidak memuat frasa terlarang.

---

## 6. Kesimpulan

Dengan terbuktinya seluruh langkah pada skenario **R-16**:
- Keterangan saat jadwal jaga kosong (*"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."*) terbukti tampil di layar.
- Judul pemilih dokter di luar jadwal jaga terbukti memandu perawat untuk mengisi alasan.
- Penetapan dokter di luar jadwal jaga saat roster kosong berhasil disimpan dengan `isRosterOverride=true`.
- Banner DPJP menampilkan catatan override dengan alasan yang sesuai.
- Seluruh 8 acceptance criteria pada modul frontend **FE-IGD-037** (khususnya Acceptance 7) kini telah terbukti secara tuntas.
