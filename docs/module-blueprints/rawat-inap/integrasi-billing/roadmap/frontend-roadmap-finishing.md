# Roadmap Frontend — Integrasi Rawat Inap ↔ Billing, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `integrasi-billing/roadmap/frontend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `integrasi-billing`, kontrak **`1.1.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-frontend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `03-frontend-architecture.md` bagian 6 (`20bcb128…`), `contracts/api-contract.md` bagian 3 (`08a4be1b…`), `testing/acceptance-test-matrix.md` bagian 4 (`532c0b8e…`). Hash lengkap pada `../blueprint-manifest.md` bagian 2.1 |
| Keputusan | `RWI-DEC-157`, `167`, `170`, `186`, `187`, `192`, `221`; gate `1.10` |
| Source SHA | Frontend `f74758af5`; backend `bf5c6bde` |
| Deret ID | `FE-RWI-166` s.d. `FE-RWI-171` |
| Roadmap pendamping | `backend-roadmap-finishing.md`, `requirement-traceability-finishing.md`. Roadmap kontrak `1.0.0` (`frontend-roadmap.md`) tetap sebagai riwayat; `FE-RWI-097` s.d. `FE-RWI-100` dicabut kontrak `1.1.0` dan pencabutannya dikerjakan task di sini |

**Otorisasi eksekusi dan pembaruan bukti 6 Oktober 2026.** Pengguna memerintahkan seluruh `FE-RWI-166` s.d. `FE-RWI-171` dikerjakan sampai tuntas pada 5 Oktober 2026; perintah itu dicatat sebagai otorisasi eksekusi, sedangkan status `DRAFT` pada metadata tetap snapshot perencanaan. Hasilnya: `FE-RWI-166`, `167`, `168`, `169`, dan `171` ✅; `FE-RWI-170` 🟡 karena alasan koreksi belum dikirim `GET placements/by-episode`. Validasi bersama: `npm run lint:errors` exit 0 sesudah implementasi; `npm run test:unit` 2174 tes, 2167 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Task gelombang 1 dikerjakan walau `BE-RWI-153`, `154`, dan `155` masih 🟡: source dan endpoint-nya tersedia, yang belum adalah UAT klinis backend. Gerbang rilis di bawah tetap berlaku.

**Kebijakan verifikasi frontend.** Mengikuti `rules/frontend/test-policy.md`: bukti utama adalah verifikasi manual kontrol interaktif dengan backend berjalan, ditambah `npm run lint`, `npm run test:unit` (menjalankan suite yang sudah ada), dan `npm run build`. Menulis test unit baru bersifat **opsional**; disarankan hanya untuk logika murni seperti pemetaan status ke label atau validasi formulir. Jest dan `@testing-library` tidak dipakai. Laporan task memuat baris `AUTOMATED TEST` dan `MANUAL TEST` secara terpisah.

**Kewenangan UI.** Bentuk dialog, warna badge, ikon, dan posisi daftar di dalam kelompoknya tetap `DEV_DISCRETION` (`03-frontend-architecture.md` 6.6). Yang mengikat: peringatan kasir dengan pengakuan sekali klik, tidak ada isian PIN, tidak ada rupiah bagi peran bangsal.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
BE-RWI-153 🟡 [BE] ─┬─────────────────────────────────────────┬─> FE-RWI-167 ✅
                    │                                         │
                    │  BE-RWI-152 ✅ [BE] ─> FE-RWI-166 ✅ ─┬─┘
                    │                                       │
                    ├───────────────────────────────────────┴─> FE-RWI-168 ✅
                    │
                    └─> FE-RWI-169 ✅

BE-RWI-154 🟡 [BE] ─> FE-RWI-170 🟡

BE-RWI-155 🟡 [BE] ─> FE-RWI-171 ✅
```

`[BE]` = task backend sub-modul ini pada `backend-roadmap-finishing.md`, cermin baca-saja.

Jumlah pasangan prasyarat→task: **8**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-152` [BE] | `FE-RWI-166` |
| 1 | `BE-RWI-153` [BE] | `FE-RWI-169` |
| 1 | `BE-RWI-154` [BE] | `FE-RWI-170` |
| 1 | `BE-RWI-155` [BE] | `FE-RWI-171` |
| 2 | `FE-RWI-166`, `BE-RWI-153` [BE] | `FE-RWI-167`, `FE-RWI-168` — boleh paralel |

Seluruh task gelombang 1 baru boleh dimulai setelah task backend yang disebut naik ke ✅.

## Gerbang rilis

| Gerbang | Isi | Alasan |
|---|---|---|
| Satu jendela rilis | `FE-RWI-166`, `FE-RWI-167`, `FE-RWI-168` dirilis bersama `BE-RWI-146`, `BE-RWI-152`, `BE-RWI-153` | Backend menghapus `supervisor-override`, `confirm-physical-discharge`, `billing-details`, dan `POST financial-clearance` yang masih dipanggil frontend lama. Backend tanpa frontend → tombol lama gagal 404. Frontend tanpa backend → keluar ruangan tanpa peringatan kasir. Praktiknya `RWF-W0` dan bagian penutupan `RWF-W1` dirilis dalam satu jendela, sesuai `02-module-map.md` 7.4 ("Billing + Rawat Inap satu rilis") |
| Layar kasir | `FE-RWI-171` dirilis bersama `BE-RWI-155` | Antrean "perlu diperiksa" kosong tanpa penanda dari backend |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-166` | Status kasir di bangsal dibaca dari satu kartu tanpa rupiah; pembaca dan penulis lama dicabut | `FR-RWF-002` s.d. `004`; `RWI-DEC-167`, `170`; `AC-RWF-003` | `1.1.0` FE 6.3 `FE-INT-06`; API 3.1, 3.3 | `billing-status-badge.jsx`, `fetchInpatientBillingStatus` | Kartu `FE-INT-06`; cabut drawer rincian finansial; `FE-INP-08` jadi riwayat baca saja | `BE-RWI-152` [BE] | Kartu | Kartu | `FE-INP-08` tidak disebut kontrak frontend (gap dokumen) / Muhammad Hamzah | Kartu |
| `FE-RWI-167` | Keluar ruangan lewat satu dialog dengan peringatan kasir dan pengakuan sekali klik | `FR-RWF-006`, `007`; `RWI-DEC-186`, `187`; `AC-RWF-006`, `007` | FE 6.3 `FE-INT-01`; API 3.2 | `use-inpatient-discharge.jsx`, kartu `FE-RWI-166` | Dialog `FE-INT-01`; cabut modal override dan konfirmasi pulang fisik | `FE-RWI-166`, `BE-RWI-153` [BE] | Kartu | Kartu | Gerbang satu jendela rilis / Muhammad Hamzah | Kartu |
| `FE-RWI-168` | Penutupan episode menunggu izin kasir yang dibaca langsung; override tanpa PIN | `FR-RWF-005`, `008`; `RWI-DEC-187`; `AC-RWF-004`, `008`; `UAT-RWF-03`, `12` | FE 6.3 `FE-INT-02`; API 3.2 | `use-inpatient-closure.jsx` | Syarat izin kasir 10 detik; pesan `INP-CLS-010`/`011`/`012`; hak override dari permission | `FE-RWI-166`, `BE-RWI-153` [BE] | Kartu | Kartu | Gerbang satu jendela rilis / Muhammad Hamzah | Kartu |
| `FE-RWI-169` | Daftar pasien yang pulang sebelum izin kasir tersedia di Daftar Pantau | `FR-RWF-007`; `RWI-DEC-186` | FE 6.3 `FE-INT-03`; API 3.5 | Daftar Pantau `FE-INP-09` | Daftar baru dan kolom status kasir saat penutupan | `BE-RWI-153` [BE] | Kartu | Kartu | Urutan kelompok milik `episode-rawat-inap` / Muhammad Hamzah | Kartu |
| `FE-RWI-170` | Salah catat penempatan dapat dikoreksi dari riwayat penempatan | `FR-RWF-019`; `RWI-DEC-157`, `192` (g); `UAT-RWF-25` | FE 6.3 `FE-INT-04`; API 3.4 | Riwayat penempatan `FE-INP-04` | Tombol dan dialog koreksi; tampilan baris lama tercoret | `BE-RWI-154` [BE] | Kartu | Kartu | Pengguna salah paham koreksi vs transfer / Muhammad Hamzah | Kartu |
| `FE-RWI-171` | Kasir menangani invoice "perlu diperiksa" | `FR-RWF-012`, `015`, `017`; `RWI-DEC-192`; `INV-RWF-08`; `AC-RWF-018` | FE 6.3 `FE-INT-05`; API 3.9 | Layar invoice kasir `billing-management` | Daftar, tanda, dan penyelesaian pemeriksaan | `BE-RWI-155` [BE] | Kartu | Kartu | Layar milik modul Billing / Yasmina | Kartu |

## Kartu task

### ✅ `FE-RWI-166` — Status kasir dari satu kartu; pembaca dan penulis lama dicabut

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 6 Oktober 2026.** Keenam acceptance criteria dipetakan ke source; pemanggil `billing-details` dan `POST financial-clearance` nol; `TRC-RWF-01` ditangani dengan menjadikan `FE-INP-08` riwayat baca saja. `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Bukti: [laporan](../task/report/frontend/FE-RWI-166.md) |
| **Outcome** | Status kasir di Detail Episode, layar Pemulangan, dan ruang kerja keperawatan tampil dari satu kartu tanpa rupiah yang menyegarkan diri tiap 10 detik. Panel rincian finansial dan formulir penandaan kelayakan keuangan manual tidak ada lagi |
| **Requirement/decision** | `FR-RWF-002`, `FR-RWF-003`, `FR-RWF-004`; `RWI-DEC-167`, `RWI-DEC-170`; `AC-RWF-003`; `FIN-CON-04` |
| **Kontrak** | `1.1.0`: frontend 6.1 dan 6.3 `FE-INT-06`; API 3.1 (`billing-details` dan `POST financial-clearance` dihapus), 3.3 (`InpatientBillingStatusResponse`) |
| **Reuse** | `billing-status-badge.jsx`, `fetchInpatientBillingStatus` pada `inpatient-billing.service.js`, hook status kasir dari `FE-RWI-096` |
| **Cakupan** | 1. Kartu `FE-INT-06` menggantikan `billing-integration/discharge-clearance-gate-card.jsx` beserta alias `features/inpatient/billing-integration/DischargeClearanceGateCard.jsx`; dipasang pada `FE-INP-04`, layar Pemulangan, dan `FE-KEP-07`. 2. Empat label: `PENDING` "Menunggu kasir", `BLOCKED` "Terkendala", `CLEARED` "Disetujui kasir", `REVOKED` "Izin dicabut"; `IsReadable = false` → "Status kasir tidak dapat dibaca"; tanda "Ditutup tanpa izin kasir"; `OVERRIDDEN` dibuang. 3. Cabut `billing-financial-details-drawer.jsx`, aliasnya, tombol pemicunya di `billing-summary-card.jsx`, dan `fetchInpatientBillingDetails`. 4. Layar Kelayakan Keuangan `FE-INP-08` menjadi riwayat baca saja: formulir tandai dan pemanggilan `POST …/financial-clearance` di `use-inpatient-financial-clearance.jsx` dicabut |
| **Dependency** | `BE-RWI-152` [BE] |
| **Acceptance criteria** | 1. Kartu menampilkan keempat label sesuai `ClearanceStatus`; tidak ada label "Overridden". 2. Billing tidak terbaca → "Status kasir tidak dapat dibaca", tidak pernah "Disetujui kasir". 3. Kasir menyetujui pukul T → paling lambat T + 10 detik kartu menampilkan "Disetujui kasir" (`AC-RWF-003`). 4. Tidak ada angka rupiah pada kartu, Detail Episode, maupun ruang kerja keperawatan. 5. Pencarian kode: nol pemanggil `billing-details` dan `POST financial-clearance`. 6. Riwayat kelayakan keuangan tetap terbaca bagi pemegang `InpatientDischarge : ReadFinancialClearance` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual kartu pada empat status dan keadaan gagal baca dengan backend berjalan; pencarian kode endpoint lama. Test unit pemetaan status ke label: opsional, layak ditulis |
| **Risiko/pemilik** | Layar `FE-INP-08` milik daftar layar `episode-rawat-inap`, dan perubahannya akibat langsung API 3.1; kontrak frontend bagian 6 belum menyebutnya (dicatat sebagai gap dokumen di traceability). Pemilik: Muhammad Hamzah |
| **DoD** | Seluruh kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-166.md` memuat baris `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### ✅ `FE-RWI-167` — Keluar ruangan dengan peringatan kasir

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 6 Oktober 2026.** Ketujuh acceptance criteria dipetakan ke source; pemanggil `supervisor-override` dan `confirm-physical-discharge` nol; kiriman pertama tanpa pengakuan, 409 `INP-DEP-001` berganti menjadi "Tetap catat keluar ruangan". `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. `BE-RWI-153` masih 🟡 dan gerbang satu jendela rilis tetap berlaku. Bukti: [laporan](../task/report/frontend/FE-RWI-167.md) |
| **Outcome** | Perawat mencatat pasien meninggalkan ruangan lewat satu dialog. Bila kasir belum memberi izin atau statusnya tidak terbaca, muncul peringatan beserta kendala, dan cukup satu klik pengakuan untuk melanjutkan. Tidak ada PIN dan tidak ada isian alasan |
| **Requirement/decision** | `FR-RWF-006`, `FR-RWF-007`; `RWI-DEC-186`, `RWI-DEC-187`; `AC-RWF-006`, `AC-RWF-007` |
| **Kontrak** | Frontend 6.3 `FE-INT-01`; API 3.2 (`record-departure`, `RecordDepartureRequest`, `InpatientDepartureResponse`, 409 `INP-DEP-001`) |
| **Reuse** | `use-inpatient-discharge.jsx`; kartu dan badge dari `FE-RWI-166` |
| **Cakupan** | 1. Dialog `FE-INT-01` pada `FE-INP-04` dan Pencatatan Kepergian `FE-INP-14`. 2. Kirim `ClearanceWarningAcknowledged = false` lebih dulu; 409 `INP-DEP-001` → tampilkan peringatan dan ganti tombol menjadi "Tetap catat keluar ruangan" yang mengirim ulang dengan `true`. 3. Hasil: "Pasien tercatat keluar pukul …. Bed sudah kosong." ditambah `FollowUpWarnings` dan jumlah pemakaian alat yang ikut ditutup. 4. Cabut `supervisor-override-modal.jsx`, `physical-discharge-confirm-modal.jsx`, alias PascalCase keduanya, serta `submitSupervisorOverride` dan `confirmPhysicalDischarge` di `inpatient-billing.service.js` |
| **Dependency** | `FE-RWI-166`, `BE-RWI-153` [BE] |
| **Acceptance criteria** | 1. Status `PENDING`: klik pertama menampilkan peringatan dan kendala tanpa rupiah; "Tetap catat keluar ruangan" berhasil dan bed kosong (`AC-RWF-006`). 2. Status `CLEARED`: tercatat tanpa peringatan. 3. Billing mati: peringatan "Status kasir tidak dapat dibaca", tetap dapat dicatat dengan pengakuan (`AC-RWF-007`). 4. Tidak ada isian PIN, password, maupun alasan. 5. Tombol nonaktif selama permintaan berjalan; klik ganda tidak mengirim dua kali; 409 "sudah dicatat" tampil apa adanya. 6. Tombol hanya tampil bagi `InpatientDischarge : RecordDeparture`. 7. Pencarian kode: nol pemanggil `supervisor-override` dan `confirm-physical-discharge` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual tiga keadaan kasir (`PENDING`, `CLEARED`, Billing mati) dengan backend berjalan; pencarian kode |
| **Risiko/pemilik** | Gerbang satu jendela rilis bersama `BE-RWI-146`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked dengan `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### ✅ `FE-RWI-168` — Gerbang kasir pada Penutupan Episode

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 6 Oktober 2026.** Keenam acceptance criteria dipetakan ke source; syarat izin kasir disegarkan tiap 10 detik, penutupan mengirim `ExpectedVersion`, dan hak penutupan tanpa izin kasir dibaca dari `InpatientDischarge : CloseOverride`. `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Bukti: [laporan](../task/report/frontend/FE-RWI-168.md) |
| **Outcome** | Petugas admisi melihat lima syarat penutupan; syarat izin kasir dibaca langsung dari Billing dan menyegarkan diri tiap 10 detik. Tombol Tutup aktif hanya bila seluruh syarat terpenuhi. Penutupan tanpa izin kasir hanya bagi pemegang permission, dengan alasan tertulis, tanpa PIN |
| **Requirement/decision** | `FR-RWF-005`, `FR-RWF-008`; `RWI-DEC-187`; `AC-RWF-004`, `AC-RWF-008`; `UAT-RWF-03`, `UAT-RWF-12` |
| **Kontrak** | Frontend 6.3 `FE-INT-02`; API 3.2 (`closure-readiness`, `close`, `close-with-override`, kode `INP-CLS-010`/`011`/`012`) |
| **Reuse** | `use-inpatient-closure.jsx` (sudah memanggil `closure-readiness` dan `close-with-override`); badge dari `FE-RWI-166` |
| **Cakupan** | 1. Syarat ke-4 memakai badge dan menyegarkan diri tiap 10 detik selama layar terbuka. 2. 422 `INP-CLS-010`/`011` → banner merah berisi pesan server; 400 `INP-CLS-012` → pesan alasan. 3. Tombol "Tutup tanpa izin kasir…" disembunyikan bagi yang tidak berhak; hak dibaca dari permission `InpatientDischarge : CloseOverride`, bukan dari nama peran. 4. Istilah layar "menembus gerbang keuangan" diganti "tanpa izin kasir" |
| **Dependency** | `FE-RWI-166`, `BE-RWI-153` [BE] |
| **Acceptance criteria** | 1. Izin `PENDING` → tombol Tutup nonaktif dan syarat 4 "Menunggu kasir". 2. Kasir menyetujui → dalam 10 detik syarat 4 "Disetujui kasir" dan tombol aktif. 3. Kasir mencabut izin saat layar terbuka → tombol kembali terkunci; bila ditekan sebelum layar segar, 422 `INP-CLS-010` tampil sebagai banner (`UAT-RWF-03`, `AC-RWF-004`). 4. Override dengan alasan "..." → pesan `INP-CLS-012`; dengan alasan jelas → episode tertutup dan bertanda "Ditutup tanpa izin kasir" (`AC-RWF-008`, `UAT-RWF-12`). 5. Pengguna tanpa `CloseOverride` tidak melihat tombol override, apa pun nama perannya. 6. Tidak ada isian PIN |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua akun (kasir dan admisi) untuk jalur setuju, cabut, dan override |
| **Risiko/pemilik** | Gerbang satu jendela rilis. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `FE-RWI-169` — Daftar "Pulang sebelum izin kasir"

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 6 Oktober 2026.** Kelima acceptance criteria dipetakan ke source. Kolom Unit dan Dicatat Oleh tampil "-" karena DTO backend belum mengirim `ServiceUnitName` dan `RecordedByUserName` yang tercantum di API 3.5; tidak menahan kriteria. `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Bukti: [laporan](../task/report/frontend/FE-RWI-169.md) |
| **Outcome** | Kasir, admisi, dan kepala ruangan melihat pasien yang keluar ruangan sebelum izin kasir, lengkap dengan status kasir saat keluar dan status sekarang. Daftar penutupan tanpa izin kasir ikut menampilkan status kasir saat penutupan |
| **Requirement/decision** | `FR-RWF-007`; `RWI-DEC-186` |
| **Kontrak** | Frontend 6.3 `FE-INT-03`; API 3.5 (`departures-before-clearance`, `closures-without-financial-clearance` + `ClosureClearanceObserved`) |
| **Reuse** | Daftar Pantau `FE-INP-09` (`monitoring-tabs.jsx`, `inpatient-monitoring-constants.jsx`) |
| **Cakupan** | Daftar baru dengan saringan Unit, Periode, dan "Termasuk episode sudah ditutup"; kolom sesuai `DepartureBeforeClearanceItem`; `Unreadable` tampil "Tidak dapat dibaca"; baris menaut ke Detail Episode; kolom status kasir saat penutupan pada daftar yang sudah ada. Posisi di dalam kelompok `DEV_DISCRETION` |
| **Dependency** | `BE-RWI-153` [BE] |
| **Acceptance criteria** | 1. Episode yang keluar dengan status `PENDING` muncul dengan "Menunggu kasir" pada kolom saat keluar. 2. Sesudah kasir menyetujui, kolom "Kasir sekarang" berubah dan kolom saat keluar tetap. 3. Keadaan memuat, kosong, dan gagal sesuai skema `FE-INT-03`. 4. Tanpa `InpatientMonitoring : Read` daftar tidak tampil. 5. Tidak ada rupiah |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan data episode uji |
| **Risiko/pemilik** | Urutan kelompok Daftar Pantau milik `episode-rawat-inap` (`02-module-map.md` 3.5). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### 🟡 `FE-RWI-170` — Koreksi penempatan

| Field | Isi |
|---|---|
| **Status** | 🟡 **SEBAGIAN 6 Oktober 2026.** Kriteria 1, 2, 4, dan 5 terpetakan ke source; kriteria 3 sebagian — baris koreksi dan baris lama tercoret sudah tampil, tetapi alasan koreksi belum dapat ditampilkan di riwayat karena `GET placements/by-episode` tidak mengirim `ChangeReason`. Menunggu backend membuka alasan koreksi pada `BedPlacementResponse`; dicatat sebagai gap atas keputusan pengguna 6 Oktober 2026, tanpa wewenang tulis backend. `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Bukti sejauh ini: [laporan](../task/report/frontend/FE-RWI-170.md) |
| **Outcome** | Kepala ruangan atau petugas admisi mengoreksi salah catat kamar, bed, kelas, atau waktu langsung dari riwayat penempatan; baris lama tetap terlihat tercoret beserta alasannya |
| **Requirement/decision** | `FR-RWF-019`; `RWI-DEC-157`, `RWI-DEC-192` butir (g); `UAT-RWF-25` |
| **Kontrak** | Frontend 6.3 `FE-INT-04`; API 3.4 (`CorrectPlacementRequest`, kode `INP-COR-001` s.d. `004`) |
| **Reuse** | Riwayat penempatan pada Detail Episode `FE-INP-04` |
| **Cakupan** | Tombol Koreksi pada baris yang berlaku; dialog bed, kelas, waktu mulai/selesai, dan alasan; minimal satu field koreksi; tanda "koreksi" dan baris lama tercoret; penanganan 422 `INP-COR-001` ("Tagihan sudah difinalkan, hubungi kasir"), `002`, `004`, dan 409 `INP-COR-003` (muat ulang) |
| **Dependency** | `BE-RWI-154` [BE] |
| **Acceptance criteria** | 1. Tombol hanya tampil bagi `InpatientBedOccupancy : Correct`. 2. Alasan kosong atau tanpa satu pun field koreksi ditolak di layar sebelum dikirim. 3. Sesudah berhasil, riwayat menampilkan baris koreksi dan baris lama tercoret beserta alasan. 4. Invoice final → "Tagihan sudah difinalkan, hubungi kasir". 5. Dua pengguna bersamaan → yang kalah melihat pesan dan riwayat dimuat ulang |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual skenario `UAT-RWF-25`. Test unit validasi formulir: opsional |
| **Risiko/pemilik** | Pengguna mencampur koreksi dengan transfer; teks dialog menjelaskan bedanya. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `FE-RWI-171` — Antrean "perlu diperiksa" kasir

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 6 Oktober 2026.** Kelima acceptance criteria dipetakan ke source. Tanda "perlu diperiksa" pada Menu Pembayaran dibaca dari `review-queue` karena `InvoiceDetailResponse` belum membawa `RequiresReview`. `npm run lint:errors` exit 0, nol error; `npm run test:unit` 2167 dari 2174 lulus — 7 kegagalan sama dengan garis dasar di luar task; tujuh berkas tes task 113/113 lulus; `npm run build` `✓ Compiled successfully in 64s`. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026**. Bukti: [laporan](../task/report/frontend/FE-RWI-171.md) |
| **Outcome** | Kasir melihat invoice yang ditandai "perlu diperiksa", membaca tandanya di layar invoice, dan menyatakan pemeriksaan selesai dengan catatan |
| **Requirement/decision** | `FR-RWF-012`, `FR-RWF-015`, `FR-RWF-017`; `RWI-DEC-192`; `INV-RWF-08`; `AC-RWF-018` |
| **Kontrak** | Frontend 6.3 `FE-INT-05`; API 3.9 (`review-queue`, `review-resolution`, kode `BIL-REV-001`, `BIL-FIN-020`, `BIL-FIN-021`) |
| **Reuse** | Layar invoice kasir `billing-management` yang sudah ada |
| **Cakupan** | Daftar `review-queue` dengan saringan; tanda "Perlu diperiksa: biaya kamar manual dan otomatis" pada detail invoice; tombol "Selesai diperiksa" dengan catatan dan `RowVersion`; pesan server untuk `BIL-REV-001` serta penolakan finalisasi `BIL-FIN-020`/`021` |
| **Dependency** | `BE-RWI-155` [BE] |
| **Acceptance criteria** | 1. Invoice bertanda muncul di daftar; kosong → "Tidak ada invoice yang perlu diperiksa." 2. Tombol hanya bagi `BillingInvoice : Update`. 3. Biaya kamar dobel masih aktif → pesan `BIL-REV-001`, tanda tetap. 4. Sesudah selesai diperiksa, invoice hilang dari daftar. 5. Finalisasi invoice bertanda → pesan `BIL-FIN-020` tampil apa adanya |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual oleh pengguna kasir |
| **Risiko/pemilik** | Layar milik modul Billing (`billing-management`). Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

## Pencabutan task kontrak `1.0.0`

| Task lama | Yang dicabut | Dikerjakan oleh |
|---|---|---|
| `FE-RWI-097` ✅ | `BillingFinancialDetailsDrawer` dan pemanggilan `billing-details` | `FE-RWI-166` |
| `FE-RWI-098` ✅ | `DischargeClearanceGateCard` | `FE-RWI-166` (pengganti), `FE-RWI-167` |
| `FE-RWI-099` ✅ | `SupervisorOverrideModal` dan pemanggilan `supervisor-override` | `FE-RWI-167` |
| `FE-RWI-100` ✅ | `PhysicalDischargeConfirmModal` dan pemanggilan `confirm-physical-discharge` | `FE-RWI-167` |

Tanda ✅ pada task lama tidak diubah; task itu memang selesai untuk kontrak `1.0.0`. Yang berubah adalah kontraknya.
