# Laporan Task Backend — BE-FIN-071

**Tanggal Selesai:** 2 Oktober 2026
**Branch:** `Yasmina`
**Penulis Laporan:** Antigravity (build-module-backend)

---

## 1. Identitas Task

| Kolom | Isi |
|---|---|
| **Task ID** | `BE-FIN-071` |
| **Gelombang** | `REV-14C` — EPIC FIN-22, jalur pengiriman |
| **Outcome** | Baris kotak keluar Finance benar-benar terkirim ke Accounting, bukan menumpuk menunggu |
| **Requirement** | `FR-FIN-150`..`153`; `FIN-DEC-118`, `093`; `FIN-DES-078` |
| **Kontrak** | `FIN-INTEGRATION-1.7` §5.12.6 |
| **Dependency** | `BE-FIN-068` ✅ |

---

## 2. Backend Governance Preflight

| Dimensi | Nilai |
|---|---|
| **Area** | Corporate |
| **Module** | FinanceManagement |
| **Submodule** | AccountingIntegration |
| **Prefix** | `Fin` — `ACTIVE` |
| **Berlakunya** | `NEW CODE` — dua file baru, nol sentuhan tabel berjalan |
| **File model baru** | **Nol** |
| **Migration** | **Nol** |
| **QBE berlaku** | QBE-NAM-001, QBE-NAM-004 |
| **Gerbang** | G3 (kredensial) tidak menahan pembangunan; `Enabled = false` bawaan memastikan nol efek pada deployment mana pun |

---

## 3. Deskripsi Perubahan

### 3.1 Arsitektur dan Dua Lapis Gerbang (FIN-DES-078)

**Lapis 1 — konfigurasi (`Finance:AccountingDispatch:Enabled`):**
- Nilai bawaan: `false` — worker berhenti dengan satu baris log informatif
- Tanpa pengubahan eksplisit konfigurasi, **nol baris dikirim** dan **nol thread HTTP dibuka**
- Menambahkan kode ini ke kodebase **tidak mengubah perilaku** lingkungan mana pun

**Lapis 2 — kode (daftar `GatedEventTypeCodes`):**
Kode yang dilewati **walaupun** Lapis 1 aktif:

| Kode | Gerbang yang menahan |
|---|---|
| `PENUTUPAN-SHIFT-KASIR` | `FIN-OQ-035`, `FIN-OQ-047` |
| `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | `FIN-OQ-035`, `FIN-OQ-047` |
| `PEMBUKAAN-SHIFT-KASIR` | `FIN-OQ-047` |
| `PPN-MASUKAN-PEMBELIAN` | `FIN-OQ-020` |
| `POTONGAN-PPH23-PIUTANG` | `FIN-OQ-028` |
| `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` | `FIN-OQ-028` |
| `POTONGAN-BIAYA-BANK-PIUTANG` | `FIN-OQ-028` |
| `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | `FIN-OQ-028` |

Baris yang dilewati **tetap PENDING** — AttemptCount tidak bertambah, sehingga tidak akan ditandai FAILED.

Lapis 2 **MUST NOT dihapus** ketika Lapis 1 dinyalakan.

### 3.2 Perilaku Per Baris

- **Transaksi per baris** — satu kegagalan tidak membatalkan seluruh siklus (FIN-DEC-093)
- **Balasan 200 dan 201 sama-sama sukses** → status `ACKNOWLEDGED`, `AccountingReceiptNumber` diisi dari `AccountingEventId` (evidence/16 bagian 5)
- **Setelah MaxAttempts** → status `FAILED`
- **Setiap percobaan** dicatat sebagai baris `FinAccountingEventAttempt` (append-only)

### 3.3 Kredensial

- `AccountingInboxUrl` dan `AccountingInboxApiKey` dibaca **dari konfigurasi** (`Finance:AccountingDispatch:*`)
- **MUST NOT** ditanamkan di source code (G3, FIN-DES-078)
- Bila `AccountingInboxUrl` kosong dan `Enabled = true`, worker berhenti dengan pesan error dan nol pengiriman

### 3.4 Pola yang Dipakai Ulang

Mengikuti `AccAccountingEventSchedulerHostedService` secara penuh:
- `IServiceScopeFactory.CreateScope()` per siklus
- `PeriodicTimer` dengan minimum poll interval
- Blok `runBackgroundJobs` di `Program.cs`
- `IOptions<T>` untuk konfigurasi

---

## 4. Berkas yang Dibuat/Diubah

| Berkas | Perubahan |
|---|---|
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingDispatchWorkerOptions.cs` | **Baru** — kelas konfigurasi, Enabled = false bawaan |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingDispatchWorker.cs` | **Baru** — hosted service pengiriman, dua lapis gerbang |
| `Program.cs` | Menambahkan `Configure<FinanceAccountingDispatchWorkerOptions>` dan `AddHostedService<FinanceAccountingDispatchWorker>` di blok `runBackgroundJobs`; menambahkan `using` namespace |

---

## 5. Validasi Acceptance Criteria

| Acceptance Criteria | Status | Bukti |
|---|---|---|
| Mati secara bawaan — tanpa konfigurasi, nol pengiriman | **Terpenuhi** | `Enabled = false` pada `FinanceAccountingDispatchWorkerOptions`; `ExecuteAsync` berhenti dengan satu baris log bila `Enabled = false` |
| Melewati ketiga penanda shift dan kode belum diratifikasi | **Terpenuhi** | `GatedEventTypeCodes` memuat 8 kode; pemeriksaan di query kandidat dan di dalam transaksi baris |
| Balasan 200 dan 201 keduanya sukses | **Terpenuhi** | `sukses = code is 200 or 201` |
| `AccountingReceiptNumber` diisi dari `AccountingEventId` | **Terpenuhi** | Parsing JSON body respons untuk properti `accountingEventId`/`AccountingEventId` |
| Kegagalan mencatat `FinAccountingEventAttempt` dan dicoba ulang | **Terpenuhi** | Setiap percobaan (berhasil maupun gagal) menulis baris baru ke `FinAccountingEventAttempt`; baris tetap `PENDING` sampai `MaxAttempts` |
| Kredensial dari konfigurasi, bukan source code | **Terpenuhi** | `AccountingInboxUrl` dan `AccountingInboxApiKey` adalah properti options, bukan literal string |

---

## 6. Status Migration dan Database

- **Nol migration baru** — nol perubahan skema database
- `FinAccountingEventAttempt` yang sudah ada dipakai langsung

---

## 7. Keterbatasan yang Dicatat

| # | Keterbatasan | Keputusan |
|---|---|---|
| 1 | Gerbang G3 (mekanisme kredensial akun layanan) belum ditutup | Worker dirancang menerima credentials dari konfigurasi; pengaktifannya menunggu kredensial turun dari Platform |
| 2 | Uji terhadap kotak masuk Accounting hanya di lingkungan integrasi | Worker tidak dapat diuji end-to-end di lingkungan dev tanpa endpoint penerima Accounting |
| 3 | Satu `HttpClient` per siklus | Pola ini mengikuti prinsip kesederhanaan; bila throughput tinggi diperlukan, `IHttpClientFactory` dapat dipertimbangkan tanpa mengubah logika gerbang |

---

## 8. Cara Mengaktifkan (Setelah G3 Turun)

Tambahkan ke konfigurasi lingkungan (BUKAN appsettings.json):

```json
"Finance": {
  "AccountingDispatch": {
    "Enabled": true,
    "AccountingInboxUrl": "<URL dari Platform>",
    "AccountingInboxApiKey": "<API Key dari Platform>",
    "PollIntervalSeconds": 30,
    "BatchSize": 50,
    "MaxAttempts": 5
  }
}
```

**Ingat:** Mengaktifkan Lapis 1 tidak mengaktifkan pengiriman penanda shift atau kode yang belum diratifikasi. Lapis 2 tetap memerlukan keputusan terpisah sebelum kode dari `GatedEventTypeCodes` dihapus.

---

## 9. Task Berikutnya

- **`BE-FIN-072`** — Hosted service penjadwal snapshot saldo (bergantung pada `BE-FIN-068` yang sudah selesai)
- **`BE-FIN-073`** — Hosted service penjadwal penanda shift
- Menutup G3 bersama Platform untuk mengaktifkan kredensial pengiriman
- Menutup `FIN-OQ-035` dan `FIN-OQ-047` bersama Accounting untuk membuka gerbang penanda shift

---

## 10. Definition of Done

- [x] `FinanceAccountingDispatchWorkerOptions` dibuat — Enabled = false bawaan
- [x] `FinanceAccountingDispatchWorker` dibuat — dua lapis gerbang lengkap
- [x] Kode `GatedEventTypeCodes` memuat 8 kode yang benar
- [x] Transaksi per baris (isolasi kegagalan)
- [x] Balasan 200 dan 201 sama-sama sukses
- [x] `AccountingReceiptNumber` diisi dari `AccountingEventId`
- [x] Setiap percobaan mencatat `FinAccountingEventAttempt`
- [x] Baris yang dilewati tetap PENDING (AttemptCount tidak bertambah)
- [x] `Program.cs` diperbarui — `Configure<>` dan `AddHostedService<>` di blok `runBackgroundJobs`
- [x] Nol migration baru
- [x] Kompilasi manual diserahkan ke pengguna
- [x] Laporan tracked ini ditulis
