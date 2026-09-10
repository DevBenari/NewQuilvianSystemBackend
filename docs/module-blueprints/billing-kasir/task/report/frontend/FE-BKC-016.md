# Laporan Perubahan Frontend — `FE-BKC-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-016` |
| Judul | Subtotal Mandiri dan Subtotal Asuransi terpisah di Menu Pembayaran |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-016` (baris 239–252) |
| Trace | `BKC-DEC-062`; `FR-BKC-008`; `CAP-09` |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API baru, murni komposisi ulang field `CalculationResponse` yang sudah dikonsumsi (`grossAmount`, `administrationFeeAmount`, `roomChargeAmount`, `itemDiscount`, `primaryAmount`, `excessAmount`) |
| Wewenang UI | Ganti komposisi baris ringkasan pembayaran — tidak ada base component baru/extend, seluruhnya `REUSE` markup `dl`/`.summaryRow` yang sudah ada di berkas yang sama |
| Dependency | Tidak ada dependency backend baru (data sudah tersedia). Disarankan roadmap dikerjakan setelah `BE-BKC-021` (selesai sesi ini) supaya angka mencerminkan gating yang diperbarui — terpenuhi karena `BE-BKC-021` memang sudah dikerjakan lebih dulu pada sesi yang sama |
| Klasifikasi | `LIGHT` — skor 2 (repo 0, berkas diperiksa 0 [≤8], berkas diubah 0 [1 berkas], logika bisnis 1/Sedang — turunan aritmetika dari nilai yang sudah dihitung, bukan formula baru, kontrak API 0, database 0, keamanan 0, UI/workflow 1) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit — lihat § 8 Status Git) |
| Commit backend yang dijadikan rujukan | `fec3579` (`NewQuilvianSystemBackend`) |
| Tanggal | 3 September 2026 |
| Status | Source lengkap, lint bersih. **Verifikasi visual langsung TERHALANG** — ditemukan Menu Pembayaran untuk invoice apa pun saat ini mengembalikan HTTP 500 pada backend yang sedang berjalan, akar masalahnya BUKAN task ini (lihat § 8 *Dependency backend* — ini temuan baru, lebih mendesak dari `BE-BKC-FIX-001`) |

---

## 1. Keadaan yang ditemukan di awal

Ringkasan pembayaran di Menu Pembayaran sebelumnya menampilkan satu baris "Subtotal Tagihan"
(gross+admin+kamar-diskon item, sebelum pajak), lalu bila ada penjamin, baris terpisah "Ditanggung
Penjamin" yang MENGURANGI angka itu (`-subtotalAsuransi`). Kasir harus melakukan pengurangan mental
sendiri untuk tahu berapa besar porsi yang murni ditanggung pasien dari subtotal itu.

Nilai-nilai yang dibutuhkan (`primaryAmount`, `excessAmount`, dan subtotal pra-pajak) **sudah**
dihitung dan tersedia di variabel lokal `subtotalTagihan`/`subtotalAsuransi` pada berkas yang sama
sejak sebelumnya — task ini murni menyusun ulang tampilan dari nilai yang sudah ada.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir, pada layar Menu Pembayaran — berlaku untuk **invoice manapun**, bukan hanya
yang berpenjamin.

**Pemicu**: kasir membuka Menu Pembayaran sebuah invoice untuk melihat ringkasan sebelum memproses
pembayaran.

**Langkah**:

1. Panel "Ringkasan Pembayaran" kini menampilkan dua baris berdampingan di posisi yang sebelumnya
   ditempati "Subtotal Tagihan" tunggal: **"Subtotal Mandiri"** (porsi pra-pajak yang murni
   tanggungan pasien sendiri) dan **"Subtotal Asuransi"** (porsi pra-pajak yang ditanggung
   penjamin — sama persis dengan nilai `primaryAmount + excessAmount` yang dulu ditampilkan sebagai
   baris pengurang "Ditanggung Penjamin"). **Kedua baris selalu tampil**, termasuk untuk invoice
   pasien tunai murni (di mana "Subtotal Asuransi" menunjukkan Rp0) — konsisten untuk invoice
   manapun, tidak disembunyikan kondisional seperti baris "Ditanggung Penjamin" yang lama.
2. Baris "Pajak" dan "Total Tagihan" (subtotal + pajak + pembulatan) tetap di posisi dan nilai yang
   sama seperti sebelumnya — **tidak ada perubahan formula**.
3. Baris "Promo / Voucher" (kondisional, bila ada) dan "Penjamin Belum Terverifikasi"
   (`unresolvedCoverageAmount`, kondisional) **dipertahankan apa adanya**, termasuk urutan dan
   kondisinya.
4. Baris "Harus Dibayar Pasien" (`patientAmount`) tetap di posisi terakhir sebagai total akhir,
   tidak berubah.

**Aturan yang berlaku**: `Subtotal Mandiri + Subtotal Asuransi = Subtotal Tagihan` (nilai lama) —
identitas ini dijaga secara matematis karena `Subtotal Mandiri` dihitung sebagai
`subtotalTagihan - subtotalAsuransi`, bukan angka baru.

**Jalur tidak normal**: kalkulasi belum tersedia (`hasCalculation=false`) — perilaku sudah ada
sebelumnya, tidak disentuh task ini (seluruh baris ringkasan bergantung `currentCalculation` yang
sama).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-016`; seluruh blok
turunan nilai (`calcAmount`, `subtotalTagihan`, `subtotalAsuransi`, `harusDibayar`, dst.) dan blok
render "Ringkasan Pembayaran" pada
`menu-pembayaran-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | Variabel turunan baru `subtotalMandiri = Math.max(0, subtotalTagihan - subtotalAsuransi)`. Baris "Subtotal Tagihan" (tunggal) dan blok kondisional "Ditanggung Penjamin" diganti dua baris tak-kondisional "Subtotal Mandiri"/"Subtotal Asuransi". Baris lain (Pajak, Total Tagihan, Promo/Voucher, Penjamin Belum Terverifikasi, Harus Dibayar Pasien) tidak disentuh |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Dua baris "Subtotal Mandiri"/"Subtotal Asuransi" | `REUSE` | Markup `<div className={styles.summaryRow}><dt>...</dt><dd>...</dd></div>` — pola yang identik dipakai SETIAP baris lain di panel yang sama (Pajak, Total Tagihan, dst.). Tidak ada elemen visual baru sama sekali |

**`UI GATE`**: tidak ada — seluruh elemen `REUSE` markup yang sudah ada di berkas yang sama, tidak
ada keputusan pengguna yang perlu ditunggu.

Tidak ada perubahan alur dependensi — murni turunan nilai baru dari state yang sudah ada di
komponen yang sama, tidak menyentuh hook, Redux, atau service.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — task ini tidak mengubah state loading, mengikuti perilaku `hasCalculation` yang sudah ada |
| Kosong | `NOT APPLICABLE` — kedua baris baru selalu tampil dengan nilai (bisa Rp0), bukan kondisi kosong |
| Gagal | `NOT APPLICABLE` — tidak ada permintaan API baru dari task ini |
| Tanpa hak akses | `NOT APPLICABLE` |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah. Data berasal dari `displayedCalculation`
yang sudah dimuat sebelumnya oleh `useMenuPembayaran` (di luar scope task ini).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (1 berkas berubah) | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Buka Menu Pembayaran invoice yang ada (login sungguhan) | **Gagal total** — halaman menampilkan "Terjadi kesalahan pada server" (HTTP 500), bukan disebabkan task ini | `NEW ERROR` (backend, bukan regresi frontend) | Log Serilog: `Npgsql.PostgresException: 42703: column b0.TariffId does not exist` pada `BillingInvoicesController.GetDetail` — lihat § 8 |
| Tampilan "Subtotal Mandiri"/"Subtotal Asuransi" dengan data nyata | **Belum bisa diverifikasi visual** — terhalang error di atas | `NOT RUN` | Lihat § 8 *Dependency backend* |

Uji manual: `NOT FEASIBLE` — Menu Pembayaran untuk **invoice manapun** saat ini gagal dimuat di
backend yang sedang berjalan, akar masalahnya adalah migration `AddTariffIdToBilInvoiceItem`
(`BE-BKC-018`) belum dijalankan ke database (kolom `TariffId` ada di model C# yang sudah di-build,
tapi belum ada secara fisik di tabel Postgres) — bukan sesuatu yang bisa diperbaiki dari sisi
frontend. Perubahan task ini sudah ditinjau lewat pembacaan kode (diff kecil, murni aritmetika dari
nilai yang sudah ada, identitas `Mandiri+Asuransi=Subtotal lama` diperiksa manual) sebagai
pengganti klik-coba langsung.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna — tanpa file test); eksekusi migration database (di luar wewenang frontend maupun task
ini — lihat § 8).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Invoice dengan `patientAmount` dan `primaryAmount`>0 menampilkan dua baris terpisah dengan nominal benar | Source terpenuhi (diperiksa manual: `subtotalMandiri + subtotalAsuransi === subtotalTagihan` lama secara aljabar); **belum diverifikasi visual** | Lihat § 6 |
| DoD: tidak ada perubahan formula, murni tampilan | Terpenuhi — `subtotalTagihan`, `totalTagihan`, `harusDibayar`, `unresolvedCoverage` semuanya dihitung persis sama seperti sebelumnya; hanya `subtotalMandiri` yang baru, itu pun turunan aljabar dari dua nilai yang sudah ada | Diff § 3.2 |
| DoD: tests/lint/build lulus | Lint: terpenuhi. Test: sengaja tidak dibuat (instruksi eksplisit). Build (`next build`) penuh: belum dijalankan | Lihat § 6 |
| Risiko: `unresolvedCoverageAmount` tidak ikut hilang | Terpenuhi — blok "Penjamin Belum Terverifikasi" dipindah tanpa diubah sama sekali (posisi, kondisi, dan isinya identik) | Diff § 3.2 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Temuan mendesak, di luar scope task ini**: Menu Pembayaran untuk **invoice apa pun** saat ini gagal dimuat (HTTP 500) di backend yang sedang berjalan. Log server: `column b0.TariffId does not exist` — migration `20260903015730_AddTariffIdToBilInvoiceItem` (`BE-BKC-018`) ada sebagai source dan sudah ter-compile ke binary yang berjalan (backend di-build ulang oleh pengguna setelah sesi `BE-BKC-018`–`021`), tapi **belum pernah dijalankan ke database** — sesuai DoD `BE-BKC-018` sendiri ("DB tidak dijalankan"). Ini BUKAN bug baru dari task ini, dan BUKAN `BE-BKC-FIX-001` (itu soal filter tarif, ini soal kolom yang belum ada secara fisik) — tapi dampaknya jauh lebih luas: **seluruh alur Menu Pembayaran terhenti**, bukan cuma dropdown tarif |
| Masalah yang diketahui | `NONE` pada source task ini sendiri |
| Dependency backend | **Migration `20260903015730_AddTariffIdToBilInvoiceItem` harus dijalankan** (`dotnet ef database update` atau setara) ke database yang dipakai backend yang sedang berjalan — wewenang ini secara eksplisit di luar frontend maupun automasi task ini (governance database terpisah). Ini memblokir verifikasi visual `FE-BKC-016` DAN pemakaian Menu Pembayaran secara umum saat ini |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat (instruksi eksplisit pengguna) |
| Interupsi | Verifikasi manual terhenti oleh temuan di atas — bukan interupsi teknis sesi, melainkan penemuan blocker backend yang lebih luas dari yang diantisipasi task ini |
| Status Git | `working tree QuilvianSystemFrontendDev`. Modified (task ini): `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx`. Berkas lain yang termodifikasi/untracked pada working tree yang sama adalah milik `FE-BKC-014`/`015` (lihat laporan masing-masing) — belum staged/commit |
| Langkah berikutnya | 1) **Jalankan migration `AddTariffIdToBilInvoiceItem` ke database** — ini prasyarat untuk verifikasi visual `FE-BKC-016` DAN memulihkan Menu Pembayaran secara umum di lingkungan ini. 2) Setelah itu, DAN setelah `BE-BKC-FIX-001` di-build/restart, verifikasi visual penuh ketiganya (`FE-BKC-014`, `015`, `016`) bisa dilakukan sekaligus dalam satu sesi klik-coba. 3) Cek invoice dengan kombinasi nilai berbeda (pasien tunai murni, pasien asuransi dengan `unresolvedCoverageAmount`>0, invoice dengan promo) untuk memastikan kelima baris ringkasan tampil benar berdampingan |
