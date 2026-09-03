# Laporan Perubahan Backend — `BE-RWI-024`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> **Task ini kini ✅ SELESAI.** Seluruh butir DoD-nya hijau: test lulus (255/255) dan
> endpointnya terbukti berjalan lewat Swagger pada aplikasi tersambung PostgreSQL.
> Tandanya pada roadmap sudah dinaikkan 🟡 → ✅.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-024` |
| Judul | Kasir dapat menandai kelayakan keuangan |
| Slice | S6 — Episode dapat ditutup dan tempat tidur kembali kosong |
| Trace | `RWI-DEC-015`, `RWI-DEC-040`; `RWI-RULE-009`, `RWI-RULE-028`; api contract `POST .../financial-clearance`; `RWI-RISK-003` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-005` 🟡 |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. `RWI-RISK-003` — dinyatakan di depan, bukan di catatan kaki

**Penandaan kelayakan keuangan pada modul ini bersifat manual.** Nilainya bergantung pada
disiplin petugas kasir, **bukan** pada angka tagihan yang sebenarnya.

Alasannya: `BillingManagement` belum punya kemampuan transaksi, sehingga tidak ada sumber
angka yang dapat dibaca. `RWI-RULE-028` aturan 7 karena itu memberikan kepemilikan konsep ini
kepada Rawat Inap **sementara**.

Konsekuensinya nyata dan perlu diketahui siapa pun yang membaca angkanya:

| Yang dikatakan sistem | Artinya sesungguhnya |
| --- | --- |
| Kelayakan keuangan `Cleared` | **Seorang kasir menyatakan lunas** |
| Kelayakan keuangan `Cleared` | **Bukan** "sistem menghitung tidak ada sisa tagihan" |

Setiap baris riwayat karena itu menyimpan `IsManualMarking = true`, dan kolom itu **wajib**
ditampilkan pada layar dan laporan. Ketika `BillingManagement` operasional, topik ini kembali
sebagai Amendment Pass.

---

## 2. Apa yang dibangun

`POST /discharges/{episodeId}/financial-clearance`, memakai butir hak akses tersendiri
`InpatientFinancialClearance : Update` — bukan butir milik discharge.

Riwayatnya **bersifat menambah, bukan menimpa**. Nilai dapat berpindah bolak-balik antara
`Pending`, `Cleared`, dan `Blocked` selama episode belum ditutup, dan setiap perpindahan
tersimpan sebagai baris tersendiri bernomor urut.

> **Kenapa berbentuk riwayat.** Tagihan susulan adalah kejadian biasa: kasir menyatakan lunas
> pukul 11:00, lalu ada tindakan tambahan yang tertagih pukul 13:00 dan nilainya kembali
> `Blocked`. Bila nilainya ditimpa, tidak ada yang dapat menjelaskan kenapa pasien yang sudah
> dinyatakan lunas tiba-tiba tertahan.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Ditambah | `GetFinancialClearanceAsync`, `MarkFinancialClearanceAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | Ditambah | `MarkFinancialClearanceRequest`, `FinancialClearanceResponse`, `FinancialClearanceEntryResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Aksi `POST /{episodeId}/financial-clearance` |
| `Areas/HealthServices/InPatientManagement/Helpers/InpatientActorClaims.cs` | Ditambah | Daftar peran kasir/billing dan `IsCashierOrBilling` |

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-AUD-001`** — `InpFinancialClearance` adalah jejak database yang terpisah dari catatan
`LoggerService`. Yang pertama adalah bukti tindakan bisnis; yang kedua penelusuran teknis.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Daftar nama peran kasir masih asumsi

`InpatientActorClaims.CashierOrBillingRoles` berisi `SuperAdmin`, `Supervisor`, `Kasir`,
`Billing`, dan `Cashier`. Nama peran di repository ini adalah data yang disiapkan admin, dan
tidak ada satu pun kontrak modul ini yang menyebutkan nama sesungguhnya.

**Akibatnya bila keliru lebih besar daripada pada penjaga lain.** Kasir yang sah ditolak 403,
kelayakan keuangan tidak pernah menjadi `Cleared`, dan **pasien ikut tertahan** karena syarat
keempat penutupan tidak pernah terpenuhi. Owner: Product/Domain.

### 5.2 Kewenangan berada di service, bukan di mesin hak akses

Butir `InpatientFinancialClearance : Update` memang terpisah, sehingga mesin hak akses
sesungguhnya **dapat** menjaganya. Penjaga di service ditulis sebagai lapis kedua: ia bekerja
walaupun butir hak aksesnya keliru diberikan kepada peran lain lewat layar Role Access.

---

## 6. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 6.1 Test yang ditulis

Di dalam `InpClearanceAndFinancialTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Tiga nilai dikenali | `Kriteria1Dan3_TigaNilaiDikenaliDanSetiapPenandaanTersimpanBesertaPelakunya` | ✅ **Lulus** 26 Agu 2026 |
| 2. Penandaan tanpa catatan ditolak 400 | `Kriteria2_PenandaanTanpaCatatanDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 3. Pelaku dan waktu tersimpan | Test kriteria 1 | ✅ **Lulus** 26 Agu 2026 |
| 4. Hanya kasir atau billing yang dapat menandai | `Kriteria4_HanyaPeranKasirAtauBillingYangDapatMenandai` | ✅ **Lulus** 26 Agu 2026 |
| 5. Hanya `Cleared` yang membuka penutupan | `Kriteria5_HanyaClearedYangMembukaPenutupan` | ✅ **Lulus** 26 Agu 2026 |

Test kriteria 1 juga memeriksa `IsManualMarking` bernilai benar pada **setiap** baris — itulah
bukti bahwa `RWI-RISK-003` benar-benar terekam pada data, bukan hanya tertulis pada laporan.

Test kriteria 4 memeriksa bahwa penolakan **tidak menyisakan baris apa pun**, bukan hanya
mengembalikan 403.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dengan butir hak akses baru `InpatientFinancialClearance : Update` |
| Database | Tidak ada perubahan schema |
| Modul tetangga | Konsep kelayakan keuangan dimiliki Rawat Inap **sementara**, sampai `BillingManagement` punya kemampuan transaksi |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6 |
| **`RWI-RISK-003` — penandaan manual** | Kelayakan keuangan tidak mencerminkan tagihan yang sebenarnya | Amendment Pass ketika `BillingManagement` operasional |
| Nama peran kasir keliru | Kasir yang sah ditolak 403, dan pasien ikut tertahan | Konfirmasi Product/Domain — bagian 5.1 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Laporan menyebut `RWI-RISK-003` secara eksplisit | ✅ Bagian 1, ditulis di depan |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Konfirmasi nama peran kasir dan billing ke Product/Domain — ini menahan pasien, bukan
   hanya menahan petugas.
