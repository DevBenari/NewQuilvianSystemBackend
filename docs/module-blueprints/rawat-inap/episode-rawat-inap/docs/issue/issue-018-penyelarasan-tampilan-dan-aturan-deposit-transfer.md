# ISSUE-018 — Penyelarasan Tampilan dan Rule Deposit Admisi Transfer Pasien terhadap Standar Pasien Lama

```yaml
issue_id: ISSUE-EPS-018
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 4 Deposit Admisi Transfer Pasien (FE-INP-20 / FE-RWI-058 / FE-RWI-229)"
sumber_laporan: "Laporan Pelapor 14 Oktober 2026 disertai tangkapan layar Langkah 4 Deposit pada Admisi Transfer Pasien"
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md
```

---

## 1. Ringkasan

Laporan pengguna menyoroti ketidaksesuaian antarmuka dan aturan (*business rule*) deposit pada alur **Admisi Transfer Pasien** (Langkah 4 dari 10 alur rujukan IGD/Poliklinik). Berdasarkan analisis perbandingan langsung terhadap alur **Admisi Pasien Lama** (yang telah diperbaiki pada `ISSUE-007` / `FIX-EPS-007-02`), ditemukan bahwa alur transfer saat ini secara sengaja mengecualikan pemanggilan dan penampilan kebijakan deposit resmi (`MstDepositPolicy`), meniadakan indikator status pemenuhan pembayaran (seperti status *"Terbayar Sebagian"*, *"Belum Membayar"*, atau *"Lunas"*), dan menampilkan kontrol pemilihan metode pembayaran transfer secara kasar (*raw*) tanpa label form, tanpa layout terstandarisasi, dan tombol *"Muat Ulang Metode"* yang meluber selebar kontainer. Masalah ini menyebabkan petugas admisi kehilangan panduan batas minimum deposit rumah sakit dan membingungkan keluarga pasien saat mencatat uang muka. Seluruh temuan dapat diperbaiki pada lapisan antarmuka frontend tanpa merusak kontrak backend atau invariant `RWI-DEC-095` (deposit tidak menahan admisi).

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lampiran Terkait |
| :---: | :--- | :--- |
| **1** | *"Tolong perbaiki tampilan deposit pada transfer pasien, coba anda analisis pada deposit admisi pasien lama, ikuti tampilan ya dan rule deposit yang sudah ditentukan, coba anda analisis dulu dan berikan planning ya"* | Tangkapan layar Langkah 4 dari 10 Deposit pada Admisi Transfer IGD/Poli (`media_1791968330512_e623bde6.png`) |

---

## 3. Ringkasan Temuan

| ID Temuan | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-EPS-018-01` | 1 | Spanduk Kebijakan Deposit Resmi (`MstDepositPolicy`) Tertutup Pesan Statis pada Jalur Transfer | `BUG` / `CONTRACT_MISMATCH` | Frontend | High | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:51-81` | `FIX-EPS-018-01` |
| `ISS-EPS-018-02` | 1 | Indikator Status Pemenuhan Deposit (Terbayar Sebagian / Belum Bayar / Lunas) Ditiadakan pada Jalur Transfer | `BUG` / `CONTRACT_MISMATCH` | Frontend | High | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:112-134` | `FIX-EPS-018-02` |
| `ISS-EPS-018-03` | 1 | Tampilan Pemilihan Metode Pembayaran Transfer Mentah Tanpa Label Form dan Tombol Muat Ulang Meluber | `UI_UX` / `DESIGN_CHANGE` | Frontend | Medium | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:141-149` & capture layar | `FIX-EPS-018-03` |
| `ISS-EPS-018-04` | 1 | Teks Keterangan di Bawah Input Nominal Duplikat dan Bertumpuk | `UI_UX` | Frontend | Low | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:107-138` | `FIX-EPS-018-04` |

---

## 4. Rincian Per Temuan

### ISS-EPS-018-01 — Spanduk Kebijakan Deposit Resmi Tertutup Pesan Statis pada Jalur Transfer

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `BUG` / `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:51-81` |
| **Perbaikan** | `FIX-EPS-018-01` |

**Apa yang terjadi.**  
Ketika petugas membuka langkah Deposit pada admisi transfer, sistem tidak menampilkan informasi batas minimum deposit rumah sakit (seperti *"Ketentuan Minimum Deposit: Rp 10.000.000 • Batas Jatuh Tempo Pelunasan: 3 Hari Kerja Pasca Admisi"*). Yang muncul hanyalah pesan generik: *"Deposit saat admisi transfer: Deposit tidak menahan admisi. Jika keluarga menyerahkan uang muka, isi nominal dan metode penerimaan. Receipt diterbitkan setelah transaksi admisi berhasil."*

**Kenapa terjadi.**  
Pada `inpatient-admission-deposit-step.jsx` baris 51–81, terdapat percabangan ternary:
```jsx
// inpatient-admission-deposit-step.jsx:51
{deposit?.transfer ? (
  <InformationAlert
    variant="info"
    title="Deposit saat admisi transfer"
    message="Deposit tidak menahan admisi. Jika keluarga menyerahkan uang muka, isi nominal dan metode penerimaan. Receipt diterbitkan setelah transaksi admisi berhasil."
  />
) : policyLoading ? (
  <InformationAlert variant="info" title="Memeriksa Kebijakan Deposit" ... />
) : policyError ? (
  ...
) : minimumAmount > 0 ? (
  <InformationAlert variant="info" title={policy?.policyName ...} message={`Ketentuan Minimum Deposit: Rp ${formattedMinimum} ...`} />
) : (
  ...
)}
```
Percabangan ini membuat jalur transfer selalu menampilkan teks statis tersebut dan mematikan seluruh blok pembacaan kebijakan deposit resmi dari billing.

**Apakah ini menyimpang dari desain?**  
Menyimpang (`BUG` / `CONTRACT_MISMATCH`). Menurut blueprint PRD ke MVP `FR-RI-174` dan `FR-RI-175`, langkah deposit harus menampilkan nominal minimum menurut kebijakan per penjamin dan kelas perawatan. Pasien yang dirujuk dari IGD/Poli untuk rawat inap tetap terikat kebijakan deposit yang sama dengan pasien admisi langsung (Pasien Lama).

**Dampak nyata.**  
Petugas admisi di loket transfer tidak mengetahui apakah pasien umum memiliki kewajiban uang muka Rp 10.000.000 atau berapa batas jatuh temponya, sehingga informasi penagihan awal menjadi bias.

**Rekomendasi.**  
Satukan atau tampilkan spanduk kebijakan deposit resmi (`policyLoading`, `policyError`, `minimumAmount`, `followUpIntervalDays`) seperti pada Admisi Pasien Lama, dengan tetap menyertakan catatan tata cara transfer (bahwa receipt resmi diterbitkan setelah transaksi admisi berhasil dan admisi tidak tertahan).

---

### ISS-EPS-018-02 — Indikator Status Pemenuhan Deposit Ditiadakan pada Jalur Transfer

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `BUG` / `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:112-134` |
| **Perbaikan** | `FIX-EPS-018-02` |

**Apa yang terjadi.**  
Pada tangkapan layar pelapor, petugas mengisi nominal uang muka transfer sebesar **Rp 1.000.000**. Padahal batas minimum kebijakan adalah **Rp 10.000.000**. Pada alur Pasien Lama, sistem menampilkan kotak peringatan kuning: *"Status Pembayaran: Terbayar Sebagian — Kekurangan deposit sebesar Rp 9.000.000 akan ditagihkan dan dapat dibuatkan Surat Pernyataan Kesediaan Melunasi pada Workspace PPRI."* Namun pada transfer pasien, kotak peringatan ini sama sekali tidak muncul. Begitu pula bila keluarga belum membayar (Rp 0), status *"Belum Membayar Deposit"* tidak tampil.

**Kenapa terjadi.**  
Pada `inpatient-admission-deposit-step.jsx` baris 112 dan 126, kode secara eksplisit mengecualikan transfer dengan klausa `!deposit?.transfer`:
```jsx
// inpatient-admission-deposit-step.jsx:112
{!deposit?.transfer && minimumAmount > 0 && hasShortfall ? (
  <div style={{ marginTop: "12px" }}>
    <InformationAlert
      variant="warning"
      title={deposit.numericAmount > 0 ? "Status Pembayaran: Terbayar Sebagian" : "Status Pembayaran: Belum Membayar Deposit"}
      message={...}
    />
  </div>
) : null}

// inpatient-admission-deposit-step.jsx:126
{!deposit?.transfer && isPaidInFull ? (
  <div style={{ marginTop: "12px" }}>
    <InformationAlert variant="success" title="Status Pembayaran: Lunas Memenuhi Kebijakan" ... />
  </div>
) : null}
```

**Apakah ini menyimpang dari desain?**  
Menyimpang (`BUG` / `CONTRACT_MISMATCH`). Aturan `FR-RI-176` menegaskan bahwa pembayaran di bawah minimum menghasilkan peringatan selisih kekurangan yang akan ditagih pada Workspace PPRI, berlaku untuk seluruh episode rawat inap aktif tanpa diskriminasi apakah pasien masuk lewat transfer IGD atau poli.

**Dampak nyata.**  
Petugas dan keluarga pasien transfer tidak mendapatkan feedback resmi bahwa pembayaran Rp 1.000.000 adalah pembayaran parsial dan masih terdapat kekurangan Rp 9.000.000 yang wajib ditindaklanjuti.

**Rekomendasi.**  
Hapus pengecualian `!deposit?.transfer` sehingga indikator status pemenuhan deposit (Terbayar Sebagian, Belum Bayar, atau Lunas) aktif pada seluruh jenis admisi, termasuk transfer.

---

### ISS-EPS-018-03 — Tampilan Pemilihan Metode Pembayaran Transfer Mentah Tanpa Label Form dan Tombol Muat Ulang Meluber

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `UI_UX` / `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:141-149` & screenshot pelapor |
| **Perbaikan** | `FIX-EPS-018-03` |

**Apa yang terjadi.**  
Pada layar transfer pasien, ketika nominal uang muka diisi (misal Rp 1.000.000), muncul dropdown pilihan metode pembayaran ("Tunai"). Namun:
1. Tidak ada label field (seperti *"Metode Penerimaan Uang Muka"*). Dropdown tampak melayang tanpa konteks form.
2. Di bawah dropdown, terdapat tombol *"Muat Ulang Metode"* dengan `variant="secondary"` yang membentang selebar blok kontainer (100% width), tampak kasar dan merusak hierarki visual halaman.
3. Tidak ada penataan kartu form (*form control group*) yang rapi sesuai design tokens Quilvian.

**Kenapa terjadi.**  
Pada `inpatient-admission-deposit-step.jsx` baris 141–149:
```jsx
// inpatient-admission-deposit-step.jsx:141
{deposit?.transfer && deposit.hasAmount ? (
  <>
    <FilterSelect id="transfer-deposit-method" value={deposit.paymentMethodId}
      placeholder="Pilih metode penerimaan deposit" options={deposit.methodOptions}
      loading={deposit.methodsLoading} onChange={deposit.setPaymentMethodId} />
    {deposit.methodsError ? <InformationAlert variant="danger" message={deposit.methodsError} /> : null}
    <BaseButton variant="secondary" onClick={deposit.reloadMethods}>Muat Ulang Metode</BaseButton>
  </>
) : null}
```
Komponen `FilterSelect` dan `BaseButton` dimasukkan dalam fragment `<>` kosong tanpa pembungkus field (`formStyles.field` atau `admissionStyles.depositMethodGroup`), tanpa elemen `<label>`, dan tanpa styling tombol yang proporsional.

**Apakah ini menyimpang dari desain?**  
Menyimpang (`UI_UX` / `DESIGN_CHANGE`). Melanggar panduan konsistensi antarmuka Quilvian (`rules/frontend/base-component-catalog.md` dan `rules/frontend/ui-consistency-checklist.md`).

**Dampak nyata.**  
Tampilan layar tampak belum selesai (*unpolished*), tidak serasi dengan langkah admisi lainnya, dan membingungkan operator pengguna.

**Rekomendasi.**  
Bungkus bagian metode penerimaan dalam kontainer form terstruktur yang rapi:
- Berikan label resmi: `Metode Penerimaan Deposit *`
- Tempatkan tombol aksi muat ulang (*reload*) secara kompak dan elegan (misalnya di samping label atau tombol teks kecil/icon), bukan tombol balok selebar layar.
- Berikan pesan bantuan (*hint*) yang jelas bahwa metode penerimaan wajib diisi bila keluarga menyerahkan uang muka transfer agar kwitansi transaksi dapat diterbitkan.

---

### ISS-EPS-018-04 — Teks Keterangan di Bawah Input Nominal Duplikat dan Bertumpuk

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `UI_UX` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx:107-138` |
| **Perbaikan** | `FIX-EPS-018-04` |

**Apa yang terjadi.**  
Pada tangkapan layar, di bawah kotak input nominal terdapat dua baris teks yang saling menduplikasi maksud:
- Baris 1: *"Kosongkan bila keluarga belum membayar uang muka. Admisi tetap dapat dilanjutkan."*
- Baris 2: *"Penerimaan dan receipt deposit dicatat bersama admisi saat booking. Nominal kosong tetap boleh dilanjutkan."*

**Kenapa terjadi.**  
Baris 107 mencetak `INPATIENT_DEPOSIT_STEP_TEXT.amountHint`, lalu baris 136 mencetak teks kondisional transfer yang mengulang kalimat *"Nominal kosong tetap boleh dilanjutkan."*

**Rekomendasi.**  
Rapikan teks hint menjadi satu pesan ringkas, elegan, dan informatif tanpa perulangan kata.

---

## 5. Tanya-Jawab Pelapor

| No | Pertanyaan / Kalimat Pelapor | Jawaban Berbasis Bukti |
| :---: | :--- | :--- |
| **1** | *"Coba anda analisis pada deposit admisi pasien lama"* | **Jawaban:** Pada alur Admisi Pasien Lama, langkah deposit menampilkan: (1) Kebijakan deposit resmi dari billing (`GET /deposit-policies`) berupa batas minimum Rp 10.000.000 dan masa jatuh tempo 3 hari; (2) Kotak input rupiah yang bersih; (3) Peringatan otomatis status pembayaran (*Terbayar Sebagian*, *Belum Bayar*, atau *Lunas*) yang menghitung kekurangan secara akurat; serta (4) Tombol Lanjut yang selalu aktif (`RWI-DEC-095`). Pada alur transfer pasien, fitur kebijakan dan indikator status ini sebelumnya dimatikan karena pengecekan `!deposit?.transfer` di frontend. |
| **2** | *"Ikuti tampilan ya dan rule deposit yang sudah di tentukan"* | **Jawaban:** Kami telah menyusun rencana perbaikan yang menyelaraskan 100% tampilan dan aturan deposit alur transfer dengan alur pasien lama: menampilkan kebijakan deposit resmi, mengaktifkan peringatan kekurangan/lunas, merapikan kontrol metode penerimaan transfer menggunakan standar komponen Quilvian, dan mempertahankan invariant bahwa deposit tidak menahan jalannya admisi. |

---

## 6. Temuan Tambahan

1. **Parameter `patientClassId` pada hook `useInpatientAdmissionDeposit`**:  
   Pada admisi transfer, kelas tempat tidur pasien baru dipilih pada Langkah 6 (*Pilih Bed*). Pada Langkah 4 (*Deposit*), `patientClassId` masih kosong. Namun `fetchInpatientDepositPolicy` di backend sudah mendukung fallback kebijakan universal pasien umum (`PatientClassId: NULL`) sehingga nominal minimum default Rp 10.000.000 tetap terbaca dengan benar tanpa error.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

| No | Keputusan yang Dibutuhkan | Rekomendasi Solusi | Status |
| :---: | :--- | :--- | :---: |
| **K-01** | Penataan layout kontrol *"Muat Ulang Metode"* pada pemilihan metode pembayaran deposit transfer. | Tempatkan tombol reload sebagai tombol aksi kompak (*ghost* atau *secondary small*) di samping kanan label form, bukan tombol blok besar di bawah dropdown. | Menunggu Persetujuan |

---

## 8. Catatan Pola

- **Pola Pengecualian Cabang (`!isTransfer`):** Hindari mematikan fitur validasi atau tampilan aturan bisnis hanya karena jenis alurnya transfer. Jika aturan bisnis bersifat universal rumah sakit (seperti kebijakan minimum deposit), aturan tersebut harus diterapkan seragam di semua varian pendaftaran.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| **2026-10-14** | Pembuatan dokumen issue awal berdasarkan keluhan antarmuka deposit admisi transfer dan perbandingannya terhadap alur pasien lama | Antigravity AI |
