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
| Dependency | `BE-RWI-082` — **selesai di source**; `BE-RWI-083` — **sebagian**, langkah 5 terblokir `BE-RWI-097` |
| Klasifikasi | `MEDIUM` — satu enum baru, dua bentuk balasan baru, dua endpoint diperluas perilakunya |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **Sebagian.** `dotnet build` `0 Error(s)`. Bentuk kontraknya lengkap dan ketiga peringatan yang sumbernya tersedia benar-benar dihitung; satu peringatan dan dua angka akibat menunggu slice modul lain. Lihat bagian 6 |

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
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026 pada commit `36db5e6d`. Nol `error CS` |
| Pembentukan service provider aplikasi | Berhasil — `dotnet ef` membangun host penuh sebelum melepasnya | `PASS` | Membuktikan pendaftaran dependency baru pada `Program.cs` beserta konstruktor service yang berubah dapat di-resolve; `HostAbortedException` sesudahnya adalah perilaku normal EF design-time |
| Verifikasi proses bisnis `UAT-50` dan `UAT-51` | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta database; bersandar pada build yang dikecualikan |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.3 | Nama field, kode peringatan, dan isi `SideEffects` dibandingkan baris per baris. Dua field aditif ditambahkan dan dicatat pada bagian 4 | `PASS` dengan delta tercatat | Bagian 4 |
| Pemeriksaan "peringatan tidak menahan" pada source | `isReady = conditions.All(...)` dan `isReadyWithOverride = conditions.All(...)`. `warnings` **tidak muncul** pada satu pun perhitungan itu. `BuildClosureWarningsAsync` tidak pernah dipanggil dari `CloseEpisodeInternalAsync` | `PASS` | `InpDischargeService.Closure.cs` — `EvaluateClosureReadinessAsync` dan `CloseEpisodeInternalAsync` |
| Pemeriksaan "angka akibat dari hasil, bukan perkiraan" | `lockedDraftCount` berasal dari nilai kembalian `LockOpenDocumentsForEncounterAsync` di dalam transaksi; `billedPendingProcedureOrderCount` dihitung ulang di dalam transaksi yang sama. Tidak ada nilai yang disalin dari `BuildClosureWarningsAsync` | `PASS` | `CloseEpisodeInternalAsync` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `NEW CODE` untuk enum dan dua bentuk balasan baru | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Pemeriksaan QBE yang berlaku | Tidak ada `Trx*` baru; `ClosureWarningCode` tidak dipersistensi sehingga tidak menuntut migration; tidak ada akses `ApplicationDbContext` dari controller | `PASS` | `git diff` |
| Review diff dan scope | Enam berkas disentuh, seluruhnya di dalam `InPatientManagement` | `PASS` | `git status --short` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Catatan cara build.** Perintahnya memakai `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, dan `-p:RunAnalyzers=false` atas permintaan pemilik pekerjaan supaya build tidak membebani mesin. Solution ini kini hanya memuat satu project — folder `Tests/` sudah tidak ada — sehingga build penuh selesai 4 menit 31 detik.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta episode yang siap ditutup.

**Tidak dijalankan:** `UAT-50` dan `UAT-51`. Keduanya menuntut aplikasi berjalan beserta data klinis; **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026**. Akibatnya, perilaku "peringatan tidak menahan" baru terbukti dari pembacaan source, belum dari permintaan yang benar-benar dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Endpoint kesiapan menyebut jumlah konsep, pesanan, dan dosis yang akan terdampak | **Sebagian.** Tiga dari empat peringatan benar-benar dihitung | Konsep dibaca dari `MrcClinicalDocumentIntegrity`; pesanan belum ditagih dan sudah ditagih dibaca dari `TrxPatientProcedure`. **Dosis tidak dapat dibaca** — tabel MAR belum ada, dan itu ditandai `isMeasured = false` beserta alasannya, bukan dilaporkan nol |
| AC-2 — Peringatan **tidak** menahan penutupan; `VAL-INP-13` s.d. `VAL-INP-17` seluruhnya bersifat peringatan | Terpenuhi di source | `isReady` dan `isReadyWithOverride` dihitung dari `conditions` saja; `BuildClosureWarningsAsync` tidak pernah dipanggil dari jalur penutupan |
| AC-3 — Hasil penutupan mengembalikan ringkasan akibat yang benar-benar tersimpan, bukan yang diperkirakan sebelumnya | Terpenuhi di source **untuk angka yang langkahnya sudah terpasang** | `lockedDraftCount` dari nilai kembalian penguncian di dalam transaksi. `cancelledProcedureOrderCount` dan `cancelledFutureDoseCount` bernilai `0` karena langkah 5 dan 6 belum terpasang; alasannya ikut dikembalikan pada `notYetWiredSteps` |
| AC-4 — Penutupan yang gagal menampilkan pesan dan episode tetap `DischargePending` — `UAT-51` | **Terpenuhi di source, belum terbukti runtime** | Blok `catch` melakukan `RollbackAsync` lalu melempar ulang; tidak ada satu pun perubahan status yang tersimpan sebelum commit. `UAT-51` `NOT RUN` |

**Kenapa AC-1 dan AC-3 belum penuh.**

| Sumber peringatan | Keadaan | Sebab |
| --- | --- | --- |
| Konsep catatan dokter | **Terbaca** | `MrcClinicalDocumentIntegrity` sudah ada |
| Pesanan tindakan tertunda belum ditagih | **Terbaca** | `TrxPatientProcedure` sudah ada |
| Pesanan tindakan tertunda sudah ditagih | **Terbaca** | Sama |
| Dosis obat yang belum dicatat | **Belum terbaca** | Tabel MAR `PharmacyManagement` dibuat `BE-RWI-114` pada roadmap `keperawatan`, dan belum ada sama sekali di repository |

| Angka akibat | Keadaan | Sebab |
| --- | --- | --- |
| `lockedDraftCount` | **Nyata** | Langkah 4 terpasang — `BE-RWI-082` |
| `billedPendingProcedureOrderCount` | **Nyata** | Dihitung di dalam transaksi penutupan |
| `cancelledProcedureOrderCount` | Selalu `0` | Langkah 5 belum terpasang — `BE-RWI-083`, menunggu `BE-RWI-097` |
| `cancelledFutureDoseCount` | Selalu `0` | Langkah 6 belum terpasang — `BE-RWI-087`, menunggu `BE-RWI-114` |

Keduanya **bukan** kekurangan source pada sisi `InPatientManagement`. Bentuk kontraknya sudah utuh,
dan setiap angka yang belum nyata membawa keterangannya sendiri agar tidak terbaca sebagai fakta.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Regresi penutupan **tanpa** konsep, pesanan, maupun dosis tetap hijau | Terpenuhi pada pemeriksaan source — seluruh peringatan berjumlah `0` dan penutupan tidak berubah jalurnya; **belum terbukti runtime** |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `🟡` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol `error CS`. Enum `ClosureWarningCode` dan kedua bentuk balasan baru tidak menimbulkan satu pun warning |
| Masalah yang diketahui | Satu dari empat peringatan dan dua dari empat angka akibat belum nyata — lihat bagian 6. Seluruhnya ditandai di dalam balasan, bukan didiamkan |
| Risiko tersisa | **Layar dapat salah membaca angka nol.** Bila frontend mengabaikan `isMeasured` dan `notYetWiredSteps`, petugas akan membaca "0 dosis akan dibatalkan" sebagai jaminan padahal langkahnya belum berjalan. Keduanya wajib dibaca `FE-RWI-065` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Jalankan `UAT-50` dan `UAT-51` pada lingkungan yang punya data klinis lalu tempelkan hasilnya ke bagian 5. Lengkapi peringatan dosis ketika `BE-RWI-114` mendarat, dan `cancelledProcedureOrderCount` ketika `BE-RWI-097` mendarat |
