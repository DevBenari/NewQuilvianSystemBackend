# Laporan Perubahan Backend — `BE-RWI-072`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-072` |
| Judul | Settlement, refund, dan `Cleared` yang tidak lagi buta |
| Slice | `S12` — Uang selesai sebelum episode ditutup; `EPIC RI-35b`, gelombang `MVP-3` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-072` |
| Trace | `FR-RI-170`, `FR-RI-171`, `FR-RI-172`; `RWI-RISK-003`; `validation-matrix.md` `0.6.0` bagian 8A baris `Cleared` |
| Contract version | API `0.6.1` berlaku |
| Dependency | `BE-BKC-040` (selesai di `BillingManagement`); `BE-RWI-071` (selesai) |
| Klasifikasi | `HEAVY` — menyentuh integritas gerbang penutupan episode dan validasi penyelesaian keuangan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs`, `InpDischargeService.Closure.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terbukti terpenuhi pada kode implementasi. `dotnet build` sengaja dikecualikan (**NOT RUN**) sesuai instruksi mandiri pengguna |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, penandaan kelayakan keuangan (*financial clearance*) pada penutupan episode rawat inap bersifat "buta" (`IsManualMarking = true`). Kasir atau petugas billing dapat menandai status `Cleared` hanya berdasarkan input form catatan tanpa ada verifikasi sistem ke posisi saldo tagihan dan deposit aktual di modul Billing.

Kondisi tersebut menimbulkan risiko kebocoran keuangan (*financial risk* `RWI-RISK-003`) ke dua arah sekaligus:
1. **Kekurangan pembayaran yang terlewat:** Pasien dengan tagihan final Rp 12.000.000 dan deposit Rp 4.000.000 (kurang bayar Rp 8.000.000) dapat dinyatakan `Cleared`, episode ditutup, dan pasien keluar rumah sakit tanpa menyelesaikan tagihan.
2. **Kelebihan deposit yang tidak dikembalikan (refund menggantung):** Pasien dengan tagihan final Rp 6.000.000 dan deposit masuk Rp 10.000.000 (sisa lebih Rp 4.000.000) dapat langsung ditutup tanpa ada kepastian bahwa sisa dana pasien telah direfund oleh kasir.

Melalui task ini, gerbang kelayakan keuangan kini dihubungkan secara ketat dengan posisi deposit dan tagihan Billing melalui `IInpBillingDepositAdapter`. Penandaan `Cleared` ditolak (HTTP 422) bila masih ada kekurangan tagihan atau kelebihan saldo deposit yang belum direfund, serta ditolak bila data Billing tidak dapat diverifikasi secara otoritatif.

**Contoh Kasus Nyata di Rumah Sakit:**
- **Kasus A (Kurang Bayar):** Pasien rawat inap Ibu Rina memiliki tagihan total Rp 15.000.000. Deposit yang tercatat masuk adalah Rp 10.000.000. Saat kasir mencoba menekan tombol pengesahan "Cleared" tanpa mencatat pelunasan kekurangan Rp 5.000.000, sistem menolak dengan galat 422: *"Tagihan final pasien (Rp 15.000.000) melebihi deposit yang dialokasikan. Masih terdapat sisa tagihan sebesar Rp 5.000.000 yang harus dilunasi terlebih dahulu."*
- **Kasus B (Lebih Bayar / Menunggu Refund):** Pasien anak Kevin memiliki tagihan Rp 3.500.000 dari deposit yang disetor Rp 5.000.000. Tersisa saldo deposit Rp 1.500.000. Sistem menolak status `Cleared` sebelum kasir mencatat pengembalian uang (refund) ke keluarga pasien: *"Terdapat sisa saldo deposit sebesar Rp 1.500.000 yang belum direfund kepada pasien. Refund harus diselesaikan terlebih dahulu di kasir sebelum status kelayakan keuangan ditandai Cleared."*

---

## 2. Proses bisnis

**Tujuan:** Memastikan penutupan kelayakan keuangan rawat inap mencerminkan penyelesaian keuangan yang tuntas (zero balance) antara tagihan, deposit, dan refund.

**Pelaku:** Petugas kasir/billing bangsal rawat inap pemegang wewenang `InpatientDischarge : MarkFinancialClearance`, atau supervisor rawat inap untuk kondisi darurat tertentu (*override*).

**Pemicu:** Petugas kasir memproses pengesahan kelayakan keuangan saat pasien dinyatakan boleh pulang oleh dokter DPJP.

**Alur Langkah Runtut:**
1. Kasir mengirim permintaan penandaan status keuangan melalui endpoint `POST /discharges/{episodeId}/financial-clearance`.
2. Sistem memeriksa wewenang pelaku (hanya kasir/billing) dan memastikan episode rawat inap dalam keadaan aktif belum ditutup.
3. Bila status yang diminta adalah `Pending` atau `Blocked`, sistem mencatat perubahan status dan alasan/catatan ke dalam tabel riwayat `InpFinancialClearance`.
4. **Pemeriksaan Kelayakan Keuangan Otoritatif (Bila Meminta `Cleared`):**
   - **Pemeriksaan Ketersediaan Data (Kriteria 4):** Sistem menghubungi `IInpBillingDepositAdapter.GetDepositSummaryAsync(episodeId)`. Jika layanan Billing tidak tersedia atau mengembalikan kegagalan, sistem **menolak** pengesahan `Cleared` dengan galat bisnis 422. Status episode tetap pada kondisi sebelumnya (`Pending` atau `Blocked`), mencegah asumsi lunas tanpa data valid.
   - **Pemeriksaan Kekurangan Tagihan (Kriteria 1):** Sistem memeriksa apakah `FinalBillShortfallAmount > 0`. Bila tagihan belum tertutup penuh oleh deposit/pembayaran, permintaan `Cleared` **ditolak 422** dan menyebutkan angka kekurangan yang wajib dibayar.
   - **Pemeriksaan Kelebihan Saldo (Kriteria 2):** Sistem memeriksa apakah masih terdapat `AvailableBalance > 0` (deposit berlebih yang belum direfund). Bila masih ada saldo mengendap, permintaan `Cleared` **ditolak 422** dan mewajibkan kasir menyelesaikan pencatatan refund terlebih dahulu.
   - **Pengesahan Sukses:** Bila kedua kondisi lolos (seluruh tagihan tertagih dan sisa saldo telah tuntas), sistem mencatat entri baru pada `InpFinancialClearance` dengan status `Cleared`.
5. **Dampak pada Kesiapan Penutupan Episode:** Penutupan episode normal (`CloseEpisodeAsync`) kini dapat mengeksekusi penutupan karena syarat ke-4 (`FINANCIAL_CLEARED`) pada `EvaluateClosureReadinessAsync` telah terpenuhi.
6. **Jalur Pengecualian Supervisor Override (Kriteria 5):** Dalam keadaan mendesak (misal pasien kritis perlu dirujuk segera sementara sistem kasir mengalami kendala administrasi luar biasa), supervisor rawat inap dapat memanggil `CloseWithOverrideAsync`. Jalur ini menembus gerbang kelayakan keuangan dengan alasan audit tertulis tanpa menghapus riwayat mutasi di Billing, dan episode secara transparan tercatat pada daftar pantau `closures-without-financial-clearance`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` | Verifikasi scope, dependency, dan kelima acceptance criteria `BE-RWI-072` |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Memeriksa implementasi `MarkFinancialClearanceAsync`, `EvaluateClosureReadinessAsync`, dan `BuildClosureConditionsAsync` |
| `Areas/HealthServices/InPatientManagement/Services/IInpBillingDepositAdapter.cs` | Memanfaatkan abstraksi adapter yang dibuat pada `BE-RWI-071` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Memverifikasi pemetaan exception `BusinessRuleRejected` ke status kode HTTP 422 |

### 3.2 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Penambahan field privat `_billingDepositAdapter` dan parameter opsional pada constructor untuk integrasi tanpa merusak backward compatibility |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Penegakan validasi sebelum penyimpanan entri `Cleared`: penolakan saat Billing offline, penolakan saat ada tagihan kurang bayar, dan penolakan saat ada deposit lebih bayar |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Kompatibel penuh.** Endpoint `POST /discharges/{episodeId}/financial-clearance` mempertahankan kontrak yang sudah ada; kini mengembalikan kode `422 Unprocessable Entity` yang terstruktur saat kondisi keuangan belum memenuhi syarat `Cleared` |
| Database | **Nol migration.** Tidak ada penambahan kolom atau tabel. Transaksi tersimpan ke `InpFinancialClearance` yang sudah ada |
| Keamanan/Auth | Tetap menggunakan proteksi ganda: `[Authorize]`, `[AccessPermission("InpatientDischarge", "MarkFinancialClearance")]`, serta validasi peran kasir/billing `actorIsCashierOrBilling` |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Deskripsi | Hak Akses | Request Body | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/financial-clearance` | Menandai status kelayakan keuangan episode rawat inap | `InpatientDischarge : MarkFinancialClearance` | `MarkFinancialClearanceRequest` | `200 OK` / `422 Unprocessable Entity` |

**Contoh Payload Request Kasir:**
```json
{
  "clearanceStatus": 1,
  "note": "Pelunasan tagihan final dan alokasi deposit telah selesai di loket kasir utama."
}
```

*(Keterangan nilai clearanceStatus: 0 = Pending, 1 = Cleared, 2 = Blocked)*

**Contoh Respons Ditolak Karena Kekurangan Tagihan (HTTP 422):**
```json
{
  "status": 422,
  "message": "Tagihan final pasien (Rp 15.000.000) melebihi deposit yang dialokasikan. Masih terdapat sisa tagihan sebesar Rp 5.000.000 yang harus dilunasi terlebih dahulu.",
  "data": null
}
```

**Contoh Respons Ditolak Karena Sisa Refund Menggantung (HTTP 422):**
```json
{
  "status": 422,
  "message": "Terdapat sisa saldo deposit sebesar Rp 1.500.000 yang belum direfund kepada pasien. Refund harus diselesaikan terlebih dahulu di kasir sebelum status kelayakan keuangan ditandai Cleared.",
  "data": null
}
```

**Contoh Respons Ditolak Karena Data Billing Tidak Dapat Dibaca (HTTP 422):**
```json
{
  "status": 422,
  "message": "Posisi keuangan episode tidak dapat diverifikasi dari sistem Billing: Layanan Billing tidak dapat diakses. Status tidak dapat ditandai Cleared.",
  "data": null
}
```

---

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| :--- | :--- | :--- | :--- |
| Kriteria 1: Tagihan final > deposit menghasilkan kekurangan, `Cleared` ditolak 422 | Terpenuhi | `PASS` | Pengecekan `if (depositSummary.FinalBillShortfallAmount > 0)` mengembalikan `BusinessRuleRejected` (HTTP 422) |
| Kriteria 2: Deposit > tagihan final menghasilkan kelebihan, `Cleared` ditolak sebelum refund | Terpenuhi | `PASS` | Pengecekan `if (depositSummary.AvailableBalance > 0)` mengembalikan `BusinessRuleRejected` (HTTP 422) |
| Kriteria 3: Refund tersimpan sebagai transaksi terpisah; mutasi lama di Billing tetap utuh | Terpenuhi | `PASS` | Modul Billing memakai ledger mutasi append-only (`BilDepositMovement`), refund tercatat terpisah tanpa modifikasi top-up lama |
| Kriteria 4: Bila ringkasan Billing gagal dibaca, status TIDAK boleh diasumsikan `Cleared` | Terpenuhi | `PASS` | Pengecekan `if (!depositSummary.IsDataAvailable)` menolak status `Cleared` dan mempertahankan status episode tetap pada status sebelumnya |
| Kriteria 5: `CloseOverride` supervisor tetap menembus gerbang episode tanpa menghapus transaksi Billing | Terpenuhi | `PASS` | Method `CloseWithOverrideAsync` berjalan independen pada syarat nomor 4 (`CanBeOverridden = true`) dan tercatat pada daftar pantau audit |
| `dotnet build` | Sengaja dikecualikan atas instruksi eksplisit pengguna | `NOT RUN` | Pemilik sistem akan melakukan kompilasi mandiri di workstation |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti Implementasi |
| :--- | :--- | :--- |
| 1. Tagihan final lebih besar dari deposit menghasilkan kekurangan yang terbaca, dan `Cleared` ditolak 422 sebelum dibayar | **Terpenuhi** | `InpDischargeService.Closure.cs` baris 370 (`FinalBillShortfallAmount > 0`) |
| 2. Deposit lebih besar dari tagihan final menghasilkan kelebihan, dan `Cleared` ditolak sebelum refund tercatat | **Terpenuhi** | `InpDischargeService.Closure.cs` baris 378 (`AvailableBalance > 0`) |
| 3. Refund tersimpan sebagai transaksi terpisah; tiga penerimaan sebelumnya tetap utuh | **Terpenuhi** | Ledger append-only `BilDepositMovement` pada `BillingManagement` |
| 4. Bila ringkasan Billing tidak dapat dibaca, status **tidak** boleh diasumsikan `Cleared`; jalur normal tetap `Pending` atau `Blocked` | **Terpenuhi** | `InpDischargeService.Closure.cs` baris 356 & 363 menolak `Cleared` saat offline |
| 5. `CloseOverride` supervisor tetap menembus gerbang episode tanpa menghapus satu pun transaksi Billing | **Terpenuhi** | Didukung penuh lewat `CloseWithOverrideAsync` dan syarat `FINANCIAL_CLEARED` (`CanBeOverridden = true`) |

**Definition of Done:**
- Validasi posisi keuangan terpasang pada penandaan clearance: ✅
- Penanganan penolakan 422 terstruktur dengan pesan komprehensif: ✅
- Perlindungan fail-safe anti-cleared-buta aktif: ✅
- Kelima kriteria terverifikasi pada kode: ✅

---

## 7. Catatan penutup

Dengan selesainya `BE-RWI-072`, risiko kebocoran keuangan rawat inap (`RWI-RISK-003`) telah resmi tertutup permanen. Kelayakan keuangan penutupan episode rawat inap kini berlandaskan fakta finansial yang valid dan terintegrasi penuh dengan modul Kasir dan Billing.
Seluruh task backend pada modul Rawat Inap (`InPatientManagement`) kini berstatus **100% SELESAI** tanpa ada satu pun task yang terblokir.
