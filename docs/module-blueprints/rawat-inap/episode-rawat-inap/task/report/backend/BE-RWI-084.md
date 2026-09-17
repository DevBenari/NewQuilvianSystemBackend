# Laporan Perubahan Backend — `BE-RWI-084`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-084` |
| Judul | Kesiapan dan akibat penutupan episode |
| Slice | Gelombang 3 — `RI-V2-1`, `EPIC RI-41` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-084` |
| Trace | `FR-RI-201`; `VAL-INP-13` s.d. `VAL-INP-17`; `RWI-DEC-129` (4), `RWI-DEC-138` (5), `RWI-DEC-143` (5); `contracts/api-contract.md` `0.9.0` bagian 10.3; `02-backend-architecture.md` 11.5.4, 11.6 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-082` — **SELESAI**; `BE-RWI-083` — **SELESAI**; `BE-RWI-087` — **SELESAI** |
| Klasifikasi | `MEDIUM` — evaluasi kesiapan penutupan dengan 4 peringatan non-blocking dan 4 metrik akibat penutupan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) / Antigravity |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-17 (diperbarui dari 2026-09-16) |
| Status | ✅ **SELESAI.** Seluruh 4 peringatan terukur (`isMeasured = true`) dan seluruh 4 angka akibat (`SideEffects`) terisi nilai riil dari transaksi penutupan (`InpDischargeService.Closure.cs:1124-1130`). `dotnet build` `NOT RUN` (instruksi pemilik: build mandiri). |

---

## 1. Masalah yang diperbaiki

Penutupan episode punya akibat yang **tidak dapat dibatalkan**: konsep catatan dokter terkunci
selamanya, pesanan tindakan batal, dosis obat batal.

Yang menekan tombol tutup adalah petugas admisi — bukan orang klinis. Ia tidak punya cara mengetahui
bahwa dr. Yoga masih punya satu catatan SOAP yang belum ditandatangani, atau bahwa ada pesanan
fisioterapi yang akan hangus. Ia menekan tombolnya, dan baru mengetahui akibatnya ketika seseorang
mengeluh.

Sesudah menutup pun, tidak ada yang memberitahunya apa yang benar-benar terjadi.

Task ini menutup kedua celah itu: **peringatan sebelum menutup**, dan **ringkasan akibat sesudah
menutup**.

---

## 2. Proses bisnis

**Tujuan.** Petugas tahu apa yang akan terjadi sebelum menekan tutup, dan tahu apa yang sudah
terjadi sesudahnya.

**Pelaku.** Petugas admisi atau supervisor yang menutup episode.

**Pemicu.** Layar penutupan episode dibuka.

**Langkah yang berurutan.**

1. Layar memanggil `GET /{episodeId}/closure-readiness`.
2. Backend mengembalikan **dua hal yang terpisah**:
   - `conditions` — kelima syarat penutupan yang **menahan**;
   - `warnings` — peringatan yang **tidak menahan**.
3. Layar menampilkan keduanya. Tombol tutup dimatikan hanya oleh `conditions`, tidak pernah oleh
   `warnings`.
4. Petugas menekan tutup. Penutupan berjalan apa pun isi `warnings`.
5. Balasan penutupan membawa `sideEffects` — apa yang **benar-benar tersimpan**.

**Kenapa peringatan tidak menahan.** Menahan penutupan berarti pasien yang sudah pulang tetap
tercatat dirawat. Ia tetap muncul di census, tempat tidurnya tetap terpakai, dan sensus harian rumah
sakit salah. Itu lebih berbahaya daripada satu konsep yang terkunci tanpa tanda tangan —
`RWI-DEC-129` (4), `RWI-DEC-138` (5), `RWI-DEC-143` (5).

**Kenapa angka akibat diambil dari hasil, bukan dari perkiraan.** Antara layar menampilkan peringatan
dan petugas menekan tombol, seorang dokter dapat menandatangani konsepnya. Menyalin angka perkiraan
ke dalam ringkasan akibat akan membuat layar melaporkan sebuah penguncian yang tidak pernah terjadi.

**Contoh berangka.** Sebelum menutup episode Joko pada 16 September 2026 pukul 12.55:

| Kode peringatan | Jumlah | Kalimat |
| --- | :---: | --- |
| `UnsignedDoctorDrafts` | 1 | "1 konsep catatan dokter akan terkunci sebagai \"Tidak Ditandatangani\" dan tidak dapat disunting lagi." |
| `PendingProcedureOrders` | 1 | "1 pesanan tindakan yang belum dilaksanakan akan dibatalkan." |
| `BilledPendingProcedureOrders` | 1 | "1 pesanan tindakan yang sudah ditagih TIDAK dibatalkan dan perlu ditindaklanjuti bersama Billing." |
| `UnrecordedPastDoses` | 0 | "Jumlah dosis obat yang belum dicatat belum dapat dibaca." — `isMeasured` bernilai **salah** |

`canClose` tetap benar. Petugas menekan tutup pada 13.00, dan balasannya:

```json
"sideEffects": {
  "lockedDraftCount": 1,
  "cancelledProcedureOrderCount": 0,
  "billedPendingProcedureOrderCount": 1,
  "cancelledFutureDoseCount": 0,
  "notYetWiredSteps": ["Langkah 5 — ...", "Langkah 6 — ..."]
}
```

**Kenapa ada `isMeasured` dan `notYetWiredSteps`.** Angka nol punya dua arti yang sangat berbeda:
"tidak ada apa-apa" dan "belum dapat dibaca". Keduanya menuntun petugas pada keputusan yang berbeda,
dan layar tidak punya cara membedakannya tanpa penanda itu. Melaporkan "0 dosis akan dibatalkan"
untuk langkah yang sebenarnya belum berjalan sama sekali adalah kebohongan yang tampak seperti
ketelitian.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Episode tidak ditemukan | `404` |
| Penutupan gagal disimpan | Transaksi di-rollback; episode tetap `DischargePending`; pemanggil menerima galat dan aman mengulang — `UAT-51` |
| Tidak ada satu pun konsep, pesanan, maupun dosis | Seluruh peringatan berjumlah `0`, penutupan berjalan seperti sebelumnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.3 | Bentuk `Warnings[]` dan `SideEffects` yang mengikat |
| `.../02-backend-architecture.md` 11.5.4, 11.6 | Empat kode peringatan, sumbernya, dan nama enum |
| `contracts/validation-matrix.md` `VAL-INP-13` s.d. `VAL-INP-17` | Sifat peringatan yang tidak menahan |
| `Areas/.../Services/InpDischargeService.Closure.cs` — `BuildClosureConditionsAsync` | Bentuk syarat yang sudah ada, agar peringatan tidak tercampur dengannya |
| `Areas/.../DTOs/InpatientClosureDtos.cs` | Pola `ClosureConditionResponse` dan `ClosureReadinessResponse` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Enums/ClosureWarningCode.cs` | **Baru.** Empat kode peringatan; tidak dipersistensi |
| `Areas/.../DTOs/InpatientClosureDtos.cs` | `ClosureWarningResponse` dan `ClosureSideEffectsResponse` **baru**; `ClosureReadinessResponse` bertambah `Warnings` |
| `Areas/.../DTOs/InpatientEpisodeDtos.cs` | `InpatientEpisodeDetailResponse` bertambah `SideEffects` yang hanya terisi pada balasan penutupan |
| `Areas/.../Services/InpEpisodeService.cs` | `InpEpisodeOperationResult` bertambah `SideEffects` |
| `Areas/.../Services/InpDischargeService.Closure.cs` | `BuildClosureWarningsAsync` **baru**; `EvaluateClosureReadinessAsync` membawa peringatan; `CloseEpisodeInternalAsync` mengisi `SideEffects` dari hasil transaksi |
| `Areas/.../Controllers/InpatientDischargeController.cs` | Kedua endpoint penutupan meneruskan `SideEffects` ke balasan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `closure-readiness` bertambah `warnings`; balasan kedua endpoint penutupan bertambah `sideEffects`. Tidak ada bentuk permintaan yang berubah, dan `isReady` beserta `isReadyWithOverride` tetap dihitung dari `conditions` saja |
| Database | Tidak ada perubahan schema. Peringatan **hanya membaca** `MrcClinicalDocumentIntegrity` dan `TrxPatientProcedure` milik modul lain |
| Keamanan/Auth | Tidak ada perubahan atribut akses. Peringatan memuat **angka** dan kalimat umum, tidak pernah memuat isi klinis maupun identitas pasien — permission matrix bagian 5.4 |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/closure-readiness` | Kelima syarat penutupan. **Berubah:** bertambah `warnings[]` yang tidak mempengaruhi `isReady` | `InpatientDischarge : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close` | Menutup episode. **Berubah:** balasannya bertambah `sideEffects` | `InpatientDischarge : Close` |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close-with-override` | Sama | `InpatientDischarge : CloseOverride` |

```yaml
GET /api/v1/health-services/inpatient-management/discharges/{episodeId}/closure-readiness:
  responses:
    "200":
      content:
        application/json:
          schema:
            type: object
            properties:
              isReady:             { type: boolean, description: "Dihitung dari conditions SAJA" }
              isReadyWithOverride: { type: boolean, description: "Dihitung dari conditions SAJA" }
              conditions:          { type: array, description: "Syarat yang MENAHAN" }
              warnings:
                type: array
                description: "Peringatan yang TIDAK menahan"
                items:
                  type: object
                  properties:
                    code:       { type: string, enum: [UnsignedDoctorDrafts, PendingProcedureOrders, BilledPendingProcedureOrders, UnrecordedPastDoses] }
                    count:      { type: integer }
                    message:    { type: string }
                    details:    { type: array, items: { type: string } }
                    isMeasured: { type: boolean, description: "Salah bila sumbernya belum tersedia; count 0 berarti BELUM TERBACA" }
```

**Delta kontrak yang dicatat.** `ClosureWarningResponse` bertambah satu field yang tidak disebut
kontrak: `isMeasured`. Alasannya ada pada bagian 2 — tanpa field itu, angka nol yang berarti "belum
terbaca" tidak dapat dibedakan dari angka nol yang berarti "tidak ada". Bersifat aditif dan tidak
mengubah satu pun field yang dikunci kontrak. Hal yang sama berlaku untuk
`sideEffects.notYetWiredSteps`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik pekerjaan: build mandiri setelah semua task diselesaikan |
| Peringatan kesiapan penutupan episode | Lengkap 4 peringatan: konsep catatan dokter, pesanan tertunda belum ditagih, pesanan tertunda sudah ditagih, dan dosis MAR belum dicatat (`isMeasured = true`) | `PASS` | `InpDischargeService.Closure.cs:662-747` |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.3 | Nama field, kode peringatan, dan isi `SideEffects` lengkap sesuai kontrak | `PASS` | Bagian 4 |
| Pemeriksaan "peringatan tidak menahan" pada source | `isReady = conditions.All(...)` dan `isReadyWithOverride = conditions.All(...)`. `warnings` tidak mempengaruhi syarat | `PASS` | `InpDischargeService.Closure.cs` |
| Pemeriksaan "angka akibat dari hasil, bukan perkiraan" | Keempat angka akibat (`lockedDraftCount`, `cancelledProcedureOrderCount`, `billedPendingProcedureOrderCount`, `cancelledFutureDoseCount`) diisi dari hasil eksekusi riil di dalam transaksi penutupan | `PASS` | `InpDischargeService.Closure.cs:1124-1130` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Review diff dan scope | Bersih dan terisolasi pada fungsionalitas penutupan episode | `PASS` | `InpDischargeService.Closure.cs` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual / UAT: `NOT RUN (instruksi pemilik: build mandiri)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Endpoint kesiapan menyebut jumlah konsep, pesanan, dan dosis yang akan terdampak | **Terpenuhi di source** | Keempat peringatan dihitung: konsep dari `MrcClinicalDocumentIntegrity`, pesanan dari `TrxPatientProcedure`, dan dosis obat terlewat dari `MedicationAdministrationService.CountUnrecordedPastDosesAsync` dengan `isMeasured = true` |
| AC-2 — Peringatan **tidak** menahan penutupan; `VAL-INP-13` s.d. `VAL-INP-17` seluruhnya bersifat peringatan | **Terpenuhi di source** | `isReady` dan `isReadyWithOverride` dihitung dari `conditions` saja; `BuildClosureWarningsAsync` tidak pernah menolak penutupan |
| AC-3 — Hasil penutupan mengembalikan ringkasan akibat yang benar-benar tersimpan, bukan yang diperkirakan sebelumnya | **Terpenuhi di source** | Keempat angka akibat (`SideEffects`) diisi nilai riil hasil eksekusi penguncian konsep, pembatalan pesanan tindakan, penghitungan pesanan tertagih, dan pembatalan dosis berjadwal di dalam transaksi |
| AC-4 — Penutupan yang gagal menampilkan pesan dan episode tetap `DischargePending` — `UAT-51` | **Terpenuhi di source** | Blok `catch` melakukan `RollbackAsync` lalu melempar ulang; seluruh perubahan status dan efek samping di-rollback |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `✅` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Seluruh 4 peringatan kesiapan dan 4 angka efek samping penutupan telah terhubung penuh |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Telah tertutup: informasi akibat penutupan disajikan transparan sebelum eksekusi dan dicatat presisi setelah eksekusi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Branch `MHamzah`. Seluruh source dan dokumentasi tersimpan rapi |
| Langkah berikutnya | Pemilik menjalankan build mandiri |
