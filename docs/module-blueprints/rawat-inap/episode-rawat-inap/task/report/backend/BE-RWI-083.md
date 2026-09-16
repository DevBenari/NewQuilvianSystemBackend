# Laporan Perubahan Backend — `BE-RWI-083`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-083` |
| Judul | Penutupan membatalkan pesanan tindakan tertunda |
| Slice | Gelombang 2 — `RI-V2-1`, `EPIC RI-41`, langkah penutupan 5 |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-083` |
| Trace | `FR-RI-199`; `INT-INP-09`; `RWI-DEC-143`; `contracts/api-contract.md` `0.9.0` bagian 10.4; `02-backend-architecture.md` 11.5.4 langkah 5; `data/data-dictionary.md` 18.5 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-097` [BE-DOK] — **belum mendarat.** `PatientProcedureOrderService` belum ada di repository |
| Klasifikasi | `MEDIUM` — satu endpoint daftar pantau baru, satu pembacaan lintas modul di dalam transaksi penutupan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **Sebagian.** `dotnet build` `0 Error(s)`. Daftar pantau dan perhitungannya selesai; **pembatalan pesanan (langkah 5) terblokir** `BE-RWI-097`. Lihat bagian 6 |

---

## 1. Masalah yang diperbaiki

Pasien pulang dengan satu pesanan tindakan yang belum dikerjakan. Sebelum task ini, penutupan
episode tidak melakukan apa-apa terhadap pesanan itu — ia menggantung selamanya di antrean tindakan,
menunggu pasien yang sudah tidak ada di rumah sakit.

Membatalkan **semuanya** juga salah. Sebagian pesanan itu **sudah ditagih ke pasien**; uangnya sudah
masuk. Membatalkannya berarti menghapus dasar sebuah tagihan tanpa ada yang memutuskannya.

`RWI-DEC-143` memisahkan keduanya:

| Keadaan pesanan | Yang dilakukan sistem |
| --- | --- |
| Tertunda dan **belum** ditagih | Dibatalkan beralasan tetap saat penutupan |
| Tertunda dan **sudah** ditagih | **Tidak disentuh**, lalu dimunculkan pada daftar pantau |
| Sudah `Completed` atau `Cancelled` | Tidak disentuh sama sekali |

**Contoh nyata.** Joko punya dua pesanan tertunda: fisioterapi yang belum ditagih, dan EKG yang
sudah masuk tagihan. Setelah penutupan, fisioterapi berstatus `Cancelled` beralasan "Episode ditutup
sebelum tindakan dilaksanakan", sedangkan EKG tetap dan muncul di daftar pantau supaya ada orang
yang menindaklanjutinya bersama Billing.

---

## 2. Proses bisnis

**Tujuan.** Pesanan yang tidak akan pernah dikerjakan ditutup rapi; pesanan yang sudah ditagih tidak
dihapus diam-diam.

**Pelaku.** Petugas yang menutup episode — untuk pembatalan. Petugas yang memantau daftar pantau —
untuk tindak lanjut bersama Billing.

**Pemicu.** Episode ditutup.

**Langkah yang berurutan.**

1. Petugas menutup episode.
2. **Langkah 5 penutupan** memisahkan pesanan tertunda menurut apakah tagihannya sudah terbentuk.
3. Yang belum ditagih dibatalkan beralasan tetap, di dalam transaksi penutupan.
4. Yang sudah ditagih **dibiarkan**, dan jumlahnya dikembalikan pada
   `sideEffects.billedPendingProcedureOrderCount`.
5. Petugas membuka `GET /monitoring/billed-pending-procedure-orders` dan melihat pesanan itu beserta
   pasien, episode, waktu tutup, dan nomor tagihannya.

**Apa yang membuat sebuah pesanan "sudah ditagih".** Kolom `IsBillingGenerated` pada
`TrxPatientProcedure`. Bila bernilai benar, butir tagihannya sudah terbentuk dan `BillingItemId`
beserta `BillingGeneratedAt` terisi.

**Contoh berangka.** Episode Joko ditutup 16 September 2026 pukul 13.00 dengan dua pesanan tertunda.

| Pesanan | Status | `IsBillingGenerated` | Setelah penutupan |
| --- | --- | :---: | --- |
| Fisioterapi | `Ordered` | salah | Menjadi `Cancelled` — **belum terpasang, lihat bagian 6** |
| EKG | `Ordered` | benar | Tetap `Ordered`, muncul di daftar pantau |
| Rontgen toraks | `Completed` | benar | Tidak disentuh |

`sideEffects.billedPendingProcedureOrderCount` bernilai `1`.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Episode tidak punya satu pun pesanan tertunda | Angkanya `0`, penutupan berjalan normal |
| Pembatalan gagal | Transaksi di-rollback; nol perubahan tersimpan; episode tetap `DischargePending` |
| Daftar pantau kosong | Mengembalikan daftar kosong, bukan galat |

**Apa yang daftar pantau ini tidak lakukan.** Ia **menampilkan**, bukan menyelesaikan. Apa tindak
lanjut terhadap pesanan tertagih itu belum diputuskan — `04-prd-to-mvp.md` 22.7 nomor 1 masih milik
Muhammad Hamzah bersama pemilik Billing.

**Kenapa episode yang belum ditutup tidak masuk daftar.** Pesanannya masih dapat dilaksanakan.
Memunculkannya akan mengubah daftar tindak lanjut menjadi daftar antrean tindakan biasa, dan yang
benar-benar perlu ditindaklanjuti tenggelam di antaranya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.4 | Bentuk route, query, dan isi balasan daftar pantau |
| `.../02-backend-architecture.md` 11.5.4 | Isi langkah 5 dan penempatan `GetBilledPendingProcedureOrdersAsync` |
| `data/data-dictionary.md` 18.5 | Penegasan bahwa `TrxPatientProcedure` ditulis lewat service pemiliknya |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Kolom `InpEpisodeId`, `ProcedureStatus`, `IsBillingGenerated`, `BillingItemId`, `CancelledAt`, `CancelReason` |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureStatus.cs` | Nilai `Planned`, `Ordered`, `Completed`, `Cancelled` |
| Pencarian `PatientProcedureOrderService` di seluruh repository | **Tidak ditemukan** — inilah dasar keputusan pada bagian 6 |
| `Areas/.../Controllers/InpatientMonitoringController.cs` | Pola lima daftar pantau yang sudah ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../DTOs/InpatientClosureDtos.cs` | `BilledPendingProcedureOrderQuery`, `BilledPendingProcedureOrderItem`, dan `BilledPendingProcedureOrderPagedResult` **baru** |
| `Areas/.../Services/InpDischargeService.Closure.cs` | `GetBilledPendingProcedureOrdersAsync` **baru**; perhitungan pesanan tertagih di dalam transaksi penutupan; dua konstanta alasan dan keterangan langkah yang belum terpasang |
| `Areas/.../Controllers/InpatientMonitoringController.cs` | `GET /billed-pending-procedure-orders` **baru**; controller kini juga memakai `InpDischargeService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu endpoint daftar pantau baru. Dua field `sideEffects` yang berhubungan dengan pesanan dipasang `BE-RWI-084` |
| Database | Tidak ada perubahan schema. Task ini **hanya membaca** `TrxPatientProcedure` milik `ClinicalManagement`; tidak satu baris pun ditulis dari `InPatientManagement` |
| Keamanan/Auth | `InpatientMonitoring : Read` dipakai ulang, konsisten dengan lima daftar pantau yang sudah ada. Daftar ini memuat nama pasien dan nomor rekam medis, tidak memuat isi klinis |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Monitoring

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/monitoring/billed-pending-procedure-orders` | Pesanan tindakan tertunda yang sudah ditagih pada episode `Closed` — tidak dibatalkan saat penutupan dan perlu ditindaklanjuti bersama Billing | `InpatientMonitoring : Read` |

```yaml
GET /api/v1/health-services/inpatient-management/monitoring/billed-pending-procedure-orders:
  summary: Daftar pantau pesanan tindakan tertagih yang tidak dibatalkan saat penutupan
  x-permission: "InpatientMonitoring : Read"
  parameters:
    - { name: serviceUnitId, in: query, schema: { type: string, format: uuid } }
    - { name: closedFrom,    in: query, schema: { type: string, format: date-time } }
    - { name: closedTo,      in: query, schema: { type: string, format: date-time } }
    - { name: pageNumber,    in: query, schema: { type: integer, default: 1 } }
    - { name: pageSize,      in: query, schema: { type: integer, default: 25 } }
  responses:
    "200":
      description: Daftar pesanan beserta pasien, episode, tindakan, pemesan, waktu pesan, waktu tutup, dan nomor tagihannya
```

Isi setiap barisnya: `procedureId`, `procedureCode`, `procedureName`, `procedureStatus`,
`episodeId`, `episodeNumber`, `patientId`, `patientName`, `medicalRecordNumber`, `serviceUnitId`,
`serviceUnitName`, `orderedByDoctorId`, `orderedByDoctorName`, `orderedAt`, `episodeClosedAt`,
`billingItemId`, `billingGeneratedAt`.

**Delta kontrak yang dicatat.** Kontrak menyebut kolom "penginput"; yang dikembalikan adalah
`orderedByDoctorId` / `orderedByDoctorName` dari kolom `DoctorId` pada `TrxPatientProcedure`.
Pembedaan penginput dan pemberi instruksi adalah kolom yang dibuat `BE-RWI-097`, dan belum ada.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026 pada commit `36db5e6d`. Nol `error CS` |
| Pembentukan service provider aplikasi | Berhasil — `dotnet ef` membangun host penuh sebelum melepasnya | `PASS` | Membuktikan pendaftaran dependency baru pada `Program.cs` beserta konstruktor service yang berubah dapat di-resolve; `HostAbortedException` sesudahnya adalah perilaku normal EF design-time |
| Verifikasi proses bisnis `UAT-50` | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta database; bersandar pada build yang dikecualikan |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.4 | Route, query, dan isi baris dibandingkan. Satu selisih penamaan kolom pemesan ditemukan dan dicatat pada bagian 4 | `PASS` dengan delta tercatat | Bagian 4 |
| Pemeriksaan arah tulis lintas modul | Pencarian penulisan `TrxPatientProcedure` di dalam `InPatientManagement` — hanya ditemukan dua **pembacaan** (`CountAsync` pada transaksi penutupan dan daftar pantau), nol penulisan | `PASS` | `git diff` `InpDischargeService.Closure.cs` |
| Pencarian `PatientProcedureOrderService` di seluruh repository | **Tidak ditemukan.** Dasar keputusan langkah 5 belum dipasang | `PASS` sebagai temuan | Pencarian nama berkas dan nama kelas pada seluruh `Areas/**` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `NEW CODE` untuk endpoint daftar pantau | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Review diff dan scope | Tiga berkas disentuh, seluruhnya di dalam `InPatientManagement`. Tidak satu berkas pun di `ClinicalManagement` yang diubah | `PASS` | `git status --short` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Catatan cara build.** Perintahnya memakai `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, dan `-p:RunAnalyzers=false` atas permintaan pemilik pekerjaan supaya build tidak membebani mesin. Solution ini kini hanya memuat satu project — folder `Tests/` sudah tidak ada — sehingga build penuh selesai 4 menit 31 detik.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta pesanan tindakan yang sudah dan belum ditagih.

**Tidak dijalankan:** `UAT-50`. Ia menuntut aplikasi berjalan beserta data pesanan tindakan; **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026**. Perlu diingat bahwa `UAT-50` sebagian besar menguji langkah 5 yang memang **belum terpasang**, sehingga menjalankannya sekarang pun belum dapat lulus penuh.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Pesanan tertunda yang **belum ditagih** menjadi `Cancelled` dengan alasan tetap saat penutupan | **Belum terpenuhi — terblokir `BE-RWI-097`** | Lihat "Kenapa langkah 5 belum dipasang" di bawah. Kalimat alasan tetapnya sudah disiapkan sebagai konstanta `AlasanPembatalanPesananSaatPenutupan` |
| AC-2 — Pesanan tertunda yang **sudah ditagih** tidak disentuh sama sekali | Terpenuhi di source | Tidak ada satu pun penulisan `TrxPatientProcedure` dari `InPatientManagement`. Pesanan tertagih dihitung, lalu dibiarkan |
| AC-3 — Pesanan yang sudah `Completed` atau `Cancelled` sebelumnya tidak disentuh | Terpenuhi di source | Seluruh penyaring menyebut `Planned` atau `Ordered` saja |
| AC-4 — Daftar pantau memuat persis pesanan pada kriteria 2 | Terpenuhi di source | `GetBilledPendingProcedureOrdersAsync` memakai penyaring yang **sama persis** dengan perhitungan pada transaksi penutupan — `IsBillingGenerated` benar, status `Planned`/`Ordered`, tidak terhapus — ditambah syarat episodenya `Closed` |
| AC-5 — Galat buatan → nol perubahan tersimpan | **Belum dapat diuji** | Langkah 5 belum menulis apa pun, sehingga belum ada perubahan yang perlu dibatalkan. Akan berlaku begitu langkah 5 dipasang |

**Kenapa langkah 5 belum dipasang.**

`02-backend-architecture.md` bagian 11.5.4 menuliskan langkah 5 sebagai pemanggilan
`PatientProcedureOrderService.CancelPendingOrdersForClosureAsync(episode.Id, actorUserId)`, dan
bagian 11.3 beserta `data-dictionary.md` 18.5 menegaskan bahwa langkah 4–6 **dipanggil lewat service
pemiliknya, tidak menulis tabel modul lain secara langsung**.

Hasil pemeriksaan source, kolom per kolom:

| Yang dibutuhkan | Keadaan |
| --- | --- |
| `TrxPatientProcedure.InpEpisodeId`, `ProcedureStatus`, `IsBillingGenerated` | **Sudah ada** |
| `TrxPatientProcedure.CancelledAt`, `CancelledByUserId`, `CancelReason` | **Sudah ada** |
| `TrxPatientProcedure.CancelledByEpisodeClosure` `boolean NOT NULL DEFAULT false` | **Tidak ada.** Kolom ini dirancang `dokter-rawat-inap` `data-dictionary` 13.10 dan dibuat `BE-RWI-097`. Ia yang membedakan "dibatalkan sistem saat penutupan" dari "dibatalkan orang" — `RWI-DEC-143` butir (3). Tanpa kolom itu, pembatalan otomatis tidak dapat dibedakan dari pembatalan manual pada baris yang sama |
| `PatientProcedureOrderService` | **Tidak ada.** Dibuat `BE-RWI-097`, dan task itu belum mendarat |

Ada dua jalan, dan yang kedua ditolak:

1. **Menunggu `BE-RWI-097`.** Langkah 5 dipasang sebagai satu pemanggilan ketika service-nya ada.
2. ~~Menulis `TrxPatientProcedure` langsung dari `InPatientManagement`.~~ **Ditolak.** Tabel itu
   milik `ClinicalManagement`; menulisnya dari sini melewati seluruh aturan pemiliknya, dan
   `BE-RWI-097` sendiri membawa risiko yang belum tuntas — perubahan perilaku endpoint poliklinik
   yang menuntut regresi rawat jalan.

Yang **dapat** dikerjakan tanpa menyentuh tabel milik modul lain sudah dikerjakan seluruhnya:
pembacaan, perhitungan, peringatan, ringkasan akibat, dan daftar pantau. Titik pemasangan langkah 5
ditandai di dalam source beserta alasannya, sehingga pemasangannya kelak adalah satu pemanggilan —
bukan penelusuran ulang.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `🟡` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol `error CS`. `InpatientMonitoringController` yang kini menerima dua service tidak menimbulkan galat resolusi |
| Masalah yang diketahui | Langkah 5 belum dipasang; AC-1 dan AC-5 belum terpenuhi. `sideEffects.cancelledProcedureOrderCount` karena itu selalu `0`, dan alasannya ikut dikembalikan pada `sideEffects.notYetWiredSteps` supaya angka nol tidak terbaca sebagai "tidak ada yang perlu dibatalkan" |
| Risiko tersisa | **Pesanan tertunda yang belum ditagih masih menggantung di antrean tindakan** setelah episode ditutup — persis masalah yang task ini dimaksudkan menutupnya. Daftar pantau tidak menutupi risiko ini, karena ia hanya memuat pesanan **tertagih** |
| Risiko yang terbuka di luar kendali task | Nasib pesanan tertagih setelah penutupan belum diputuskan — `04-prd-to-mvp.md` 22.7 nomor 1, menunggu Muhammad Hamzah bersama pemilik Billing. Bila keputusan itu turun, cakupan task ini dinilai ulang lebih dulu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Rantai prasyarat yang sebenarnya | **Lebih panjang dari satu task.** `BE-RWI-097` sendiri menunggu `BE-RWI-090` pada roadmap `dokter-rawat-inap`, dan `BE-RWI-090` juga belum dikerjakan — folder `dokter-rawat-inap/task/report/backend/` tidak memuat satu pun laporan pada rentang `BE-RWI-088` ke atas. Jadi urutannya `BE-RWI-090` → `BE-RWI-097` → task ini. **Gerbang persetujuannya sudah tertutup**: pemberitahuan pemilik `rawat-jalan` atas `R7` ditutup 16 September 2026 lewat `RWI-DEC-152` oleh Sukma GP, dengan catatan regresi poliklinik tetap wajib. Yang tersisa murni pekerjaan, bukan menunggu keputusan orang |
| Langkah berikutnya | Kerjakan `BE-RWI-090` lalu `BE-RWI-097` pada roadmap `dokter-rawat-inap`. Setelah `PatientProcedureOrderService` ada, pasang pemanggilannya pada titik yang sudah ditandai di `CloseEpisodeInternalAsync`, hapus `LangkahLimaBelumTerpasang` dari `notYetWiredSteps`, lalu perbarui laporan ini |
