# Laporan Perubahan Frontend — `PAY-FE-FIX-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-006` (ad-hoc, bukan kartu roadmap) |
| Judul | Penyembunyian seluruh konfigurasi dan jalur BPJS Kesehatan/Ketenagakerjaan di UI (Payroll, HRD/Benefit, K3) mengikuti satu saklar Backend, gagal-tertutup, tanpa mengirim nol untuk isian yang disembunyikan |
| Roadmap | **NOT APPLICABLE** (preseden `PAY-FE-FIX-001..005`) |
| Dependency | `PAY-BE-FIX-006` (belum dibuild) — kontrak `GET /api/v1/system/features` dan penolakan 409 |
| Repository / branch | `QuilvianSystemFrontendDev` / `yasmina`, HEAD `cde9e777158fd6fe8002b65c0e73e2c684bb5e64` |
| Model / tanggal | Claude Sonnet 5.5 / 2026-11-18 |
| Status | 🟡 **Source selesai; lint dan uji unit dijalankan. `npm run build` NOT RUN, `npm run dev` NOT RUN, `VISUAL CHECK: NOT FEASIBLE` (tidak boleh menjalankan dev server, dan Backend belum dibuild).** Perilaku visual dan jaringan belum pernah dilihat |

---

## Prinsip

1. **Backend pemilik status; FE hanya mengikuti.** Satu hook membaca `GET /api/v1/system/features`; tidak ada pengaturan fitur di layar mana pun.
2. **Gagal-tertutup.** BPJS tampil hanya bila Backend menjawab `bpjsEnabled === true` (boolean sungguhan). Belum dijawab, gagal dibaca, atau bentuk lain apa pun = nonaktif. Sebelumnya `usePayrollModuleMode` gagal-terbuka (bawaan `true`, galat membuka BPJS kembali).
3. **Tidak mengirim nol.** Isian BPJS yang disembunyikan **dihilangkan dari payload** (bukan `false/0/null`), karena menyimpan atribut non-BPJS tidak boleh menimpa nilai BPJS tersimpan. Backend menegakkan hal yang sama.
4. **Akses URL langsung.** Rute khusus BPJS dibungkus penjaga; layar dan permintaan datanya tidak dipasang sebelum fitur terbukti aktif.

## 1. Berkas baru

| Berkas | Isi |
| --- | --- |
| `lib/constants/system/system-features-constants.js` | Endpoint `/v1/system/features`, teks, keadaan tertutup |
| `utils/system/system-features-utils.js` | `normalizeSystemFeatures` (hanya `true` boolean), `createSystemFeaturesStore` (satu permintaan bersama, gagal tetap tertutup tetapi dicoba ulang, reset) |
| `lib/services/system/system-features-service.js` | `SystemFeaturesService.get` |
| `lib/hooks/system/use-system-features.js` | `useSystemFeatures` (`useSyncExternalStore`, tanpa setState di efek), `resetSystemFeaturesCache` |
| `lib/hooks/system/use-bpjs-aware-config.js` | Config master data tanpa isian klasifikasi BPJS bila nonaktif |
| `utils/system/bpjs-feature-config-utils.js` | Penyaring murni: field, baris detail, payload, kolom tabel |
| `components/features/system/bpjs-feature-guard.jsx` | Penjaga rute (memeriksa / gagal / dinonaktifkan) memakai `InformationAlert` |
| `tests/unit/bpjs-feature-deactivation.test.mjs` | 11 uji |

## 2. Audit Frontend

| Rute / menu | Komponen / form | Layanan / hook | Perubahan |
| --- | --- | --- | --- |
| `/payroll/policies/bpjs` (menu `corporatePayrollBpjsPolicy`) | `app/payroll/policies/bpjs/page.jsx` → `PayrollPolicyListView kind="bpjs"` | `PayrollPolicyService` (bersama PPh) | Halaman dibungkus `BpjsFeatureGuard`; menu disembunyikan. **Komponen kebijakan bersama (`payroll-policy-*`, konstanta, service, hook) tidak diubah**; `/payroll/policies/tax` tidak dijaga dan tetap berfungsi |
| `/corporate/hrd/benefits/cob-claims` (menu `corporateBenefitCobClaims`) | `CobClaimsWorkstationView` | — | Dibungkus penjaga; menu disembunyikan |
| `/ess/claims/cob-submission` (menu `corporateMyCobClaims`) | `EssCobSubmissionView` | — | Dibungkus penjaga; menu disembunyikan |
| Sidebar | `left-sidebar-items-virtualized.jsx`, `adaptPayrollMenuItems` | `useSystemFeatures({ enabled: permissionLoaded })` | Tidak lagi bergantung izin kebijakan; adapter gagal-tertutup (hanya `true` mempertahankan); label laporan "PPh dan BPJS" → "Laporan PPh 21" saat nonaktif |
| Profil pegawai → tab Asuransi | `workforce-insurance-config-editor.jsx` | `validateInsuranceForm/buildInsurancePayload(form, { includeBpjs })` | Bagian BPJS Kesehatan/Ketenagakerjaan, nomor, program, dan peringatan tarif disembunyikan; kolom BPJS **tidak dikirim**; asuransi swasta tetap |
| Profil pegawai → daftar | `workforce-profile-dashboard.jsx` (`ResourcePanel`) | `filterBpjsColumns` | Kolom nomor BPJS disembunyikan; data tidak dihapus |
| Data induk Jenis Tunjangan & Kebijakan Tunjangan Transportasi | form, detail | hook editor/detail (4 berkas) + `useBpjsAwareConfig` | "Tunjangan Tetap" dan "Masuk Dasar Upah BPJS" disembunyikan dari form dan detail; kunci tidak dikirim |
| Pengaturan Penggajian (legacy, tanpa rute) | `payroll-settings-view.jsx` | `payrollService.updatePayrollSettings` | Bagian 3 BPJS disembunyikan; kolom `bpjs*` dibuang dari payload saat nonaktif |
| HRD Benefit: Katalog Paket | `benefit-plans-view.jsx`, `BenefitPlanFormModal.jsx` | — | Paket Lapisan 1 BPJS disembunyikan dari daftar, filter, dan form; kolom/aturan COB BPJS disembunyikan dan tidak dikirim |
| HRD Benefit: Kepesertaan | `benefit-enrollments-view.jsx`, `PolicyNumberModal.jsx` | — | Kolom dan isian nomor BPJS disembunyikan, tidak divalidasi, tidak dikirim; polis swasta tetap |
| HRD Benefit: Dasbor, Keluarga Saya | `benefits-dashboard-view.jsx`, `ess-my-dependents-view.jsx`, `FamilyRegistrationFormModal.jsx` | — | Kartu KPI BPJS, kolom "Status Jaminan BPJS", dan teks BPJS disembunyikan |
| K3: Registrasi KAK/PAK | `ohs-injuries-view.jsx` (`BpjsJkkExportModal` hanya dipasang saat aktif) | — | Banner SLA 48 jam, kartu klaim JKK, filter, kolom status, tombol "Klaim JKK" disembunyikan; pencatatan, CAPA, LTI tetap |
| `usePayrollModuleMode`, `normalizePolicyAvailability`, `usePayrollPolicyMetadata` | — | — | Sumber tunggal = fitur sistem; bawaan `false`; `!== false` → `=== true` |
| **Tidak diubah (sengaja)** | Run/laporan/kesiapan Payroll (menampilkan hasil historis dan status `TESTING_MODE_DISABLED`), pendaftaran/kiosk/Billing/penjamin BPJS, antrean, `payroll-approvals/runs/worksheet-view` (legacy tanpa rute), `cob-claims-workstation-view`, `ess-cob-submission-view`, `ClaimSettlementDrawer` (di dalam rute yang dijaga) | — | HISTORICAL_DATA / UNRELATED / SHARED_DEPENDENCY |

## 3. Matriks Integrasi

| API Backend | DTO | Service FE | Hook | UI |
| --- | --- | --- | --- | --- |
| `GET /api/v1/system/features` | `SystemFeaturesResponse { bpjsEnabled, bpjsDisabledMessage }` | `SystemFeaturesService.get` | `useSystemFeatures`, `usePayrollModuleMode`, `useBpjsAwareConfig` | Sidebar, `BpjsFeatureGuard`, seluruh layar §2 |
| `PUT/POST` profil iuran (`WfpInsuranceController`) | `CreateWfpInsuranceRequest` (kolom BPJS dihilangkan saat nonaktif; BE mempertahankan nilai tersimpan) | payload `buildInsurancePayload` | `WorkforceInsuranceConfigEditor` | Tab Asuransi |
| `POST/PUT` master Jenis Tunjangan / Kebijakan Transportasi | request dengan `IsFixedAllowance`/`IncludeInBpjsWageBase` (dihilangkan; BE mengabaikan/menjaga) | `buildPayload` + `filterBpjsClassificationPayload` | hook editor | Form data induk |
| `PUT api/v1/corporate/hrd/benefits/enrollments/{id}/policy-numbers` | `UpdatePolicyNumbersRequestDto` (nomor BPJS dihilangkan; 409 bila dikirim) | — (layar HRD saat ini lokal) | `PolicyNumberModal` | Kepesertaan |
| `POST api/v1/corporate/hrd/benefits/claims`, `GET .../bpjs-jkk-form`, tulis `bpjs-policies` | — | — | — | Rute dijaga/tombol disembunyikan; **API menolak 409 juga bila FE dilewati** |

## 4. Hasil pemeriksaan statis

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada seluruh berkas yang diubah/baru | **0 error.** Peringatan `react-hooks/set-state-in-effect` yang tersisa sudah ada pada kode lama (efek `load…` di layar benefit/legacy dan hook master data), bukan dari perubahan ini |
| `check-css-usage.mjs` pada berkas JSX yang diubah | Berkas baru (`bpjs-feature-guard.jsx`) dan editor iuran: **0 temuan**. Total 7 temuan **sudah ada di HEAD** (`BaseButton variant="outline"` ×6, kelas `INVESTIGATION_COMPLETED` pada `ohs-injuries-view`), pada baris yang tidak diubah. Jumlah kemunculan pola yang sama di HEAD dan sekarang identik |
| `node --test tests/unit/bpjs-feature-deactivation.test.mjs` | **11/11 lulus** |
| `payroll-policy.test.mjs`, `payroll-tax-config.test.mjs` | **46/46 lulus** setelah pembaruan kontrak (lihat §5) |
| Suite penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`) | 5168 uji, 5113 lulus, **55 gagal pada 39 berkas**. Dibandingkan dengan salinan `HEAD` murni (`git archive`, tanpa stash; 5149 uji, 59 gagal): **tidak ada kegagalan yang hanya ada di working tree**. Selisih empat kegagalan hanya ada di salinan HEAD (berkas akar tak ikut diarsipkan, dan uji kontrak lama membaca Backend yang sudah diubah). Salinan sementara dihapus |
| `npm run build`, `npm run dev`, runtime, `VISUAL CHECK` | **NOT RUN / NOT FEASIBLE** |

## 5. Uji yang diperbarui karena kontrak berubah (disengaja)

- `payroll-policy.test.mjs`: bawaan `EnableBpjsInPayroll` Backend kini `false` (sebelumnya "default produksi harus true"); `normalizePolicyAvailability` gagal-tertutup; adapter menu gagal-tertutup.
- `payroll-tax-config.test.mjs`: `validateInsuranceForm/buildInsurancePayload` kini menerima `{ includeBpjs }` (bawaan `false`); kasus lama dijalankan dengan `includeBpjs: true`; kasus nonaktif ditambah.

## 6. Checklist UAT (belum dijalankan)

Prasyarat: BE dibuild, saklar `false`; lalu ulangi bagian terpilih dengan `Payroll__Policy__EnableBpjsInPayroll=true` + restart.

**Positif (saklar mati)**
1. Sidebar tidak memuat "Kebijakan Iuran BPJS", "Meja Kerja Klaim COB", "Klaim Koordinasi Manfaat (COB)"; "Kebijakan Tarif PPh" ada; laporan bernama "Laporan PPh 21".
2. Tab Asuransi pegawai: hanya status kepesertaan, asuransi swasta, masa berlaku; simpan berhasil; nilai BPJS lama di database tidak berubah (cek via SQL baca-saja).
3. Jenis Tunjangan/Kebijakan Transportasi: ubah nama/nominal tanpa menyentuh klasifikasi; klasifikasi lama utuh.
4. Payroll Run non-BPJS (gaji, PPh 21, asuransi swasta) dapat dihitung; baris BPJS berstatus mode pengujian; asuransi swasta ikut terhitung.
5. K3: catat KAK/PAK berhasil tanpa nomor klaim JKK, tanpa banner 48 jam; kolom/tombol JKK tidak ada.
6. HRD Benefit: paket swasta/internal dapat dibuat; Lapisan BPJS tidak ada; kepesertaan: polis swasta dapat diubah.

**Negatif / akses langsung**
7. Buka `/payroll/policies/bpjs`, `/corporate/hrd/benefits/cob-claims`, `/ess/claims/cob-submission` langsung: tampil pesan "dinonaktifkan", tanpa form/permintaan data.
8. Putuskan jaringan ke `/api/v1/system/features`: konfigurasi BPJS tetap tersembunyi (tidak terbuka kembali); muat ulang saat jaringan pulih memulihkan status.
9. API langsung (Postman, token valid): `POST/PUT bpjs-policies`, `POST benefits/claims`, `PUT policy-numbers` dengan nomor BPJS, `GET injuries/{id}/bpjs-jkk-form`, `POST/PUT plans` kategori BPJS → **409** dengan pesan baku; `GET` kebijakan BPJS tetap 200.

**Regresi**
10. PPh 21 (kebijakan, profil pajak, hitung) tidak berubah. Asuransi swasta di editor tidak berubah. HRD umum, Billing, Finance, Akuntansi, pendaftaran pasien dengan penjamin BPJS, antrean: tidak berubah. Riwayat run lama, klaim lama, nomor BPJS lama: utuh dan terbaca.
11. Finance Handoff pada run mode pengujian tetap ditahan (pengaman utuh).
12. Nilai saklar **sama** di FE dan BE: ubah env var, restart, `GET /system/features` dan perilaku API konsisten; worker JKK tidak berjalan saat mati.

## 7. Risiko dan keputusan tertunda

Lihat `PAY-BE-FIX-006` §6 (Finance Handoff tertahan, kewajiban hukum BPJS, bawaan saklar `true→false`, klaim COB berjalan tanpa meja kerja, nilai contoh tertanam pada formulir JKK, penghapusan permanen tidak dilakukan). Tambahan Frontend:

| # | Hal |
| --- | --- |
| 1 | Layar Benefit (paket, kepesertaan, dasbor, keluarga) masih memakai data awal lokal; penyembunyian berlaku pada tampilan, bukan data. Pengaitan ke API nyata di luar cakupan |
| 2 | `usePayrollModuleMode` tidak lagi memanggil `tax-policies/availability`; `bpjsTestingModeNotice` kini pesan baku Backend. Pemakai `taxPolicyApproved/Version` dari hook itu: tidak ada (diperiksa) |
| 3 | Berkas `transport-allowance-policy-*`, `master-data-transport-allowance-policy-slice`, `menu-items.jsx`, dan `payroll-transport-allowance-policy.test.mjs` pada working tree **diubah pihak lain secara paralel**, bukan oleh task ini |
