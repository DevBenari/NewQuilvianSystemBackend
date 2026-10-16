# Roadmap Frontend Transfer Pasien Masuk (IGD/Rajal ke Ranap) & Penugasan PPJA

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` (COMPOSITE; peta menu pada [module map 10.2](../../02-module-map.md)) |
| planning_id / revision / tanggal | `TRF-DEL-20261016-01` / 1 / 2026-10-16 Asia/Jakarta |
| status / approval | Desain disetujui pengguna 16 Oktober 2026 ("iya lakukan /plan-module-delivery"). Kontrak target `0.14.0`. Setiap task dieksekusi hanya lewat `build-module-frontend` dengan task ID-nya |
| blueprint / kontrak | [Frontend 17](../03-frontend-architecture.md), [Kontrak Transfer](../contracts/admission-transfer-patient-handover-contract.md), [PRD 28](../04-prd-to-mvp.md), [Flowchart 19](../flowcharts/19-alur-transfer-pasien-masuk-igd-rajal-ke-ranap.md) |
| input revisions | Decision log 51; gate 1.13; module map 7; manifest anak 13 draft; kontrak target `0.14.0` |
| source SHA frontend / backend | `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` / `ca09cae909597442385ce4513be9c803549be10e` |
| hash lock | [transfer-pasien-masuk-delivery-lock.json](transfer-pasien-masuk-delivery-lock.json) |
| scope / ID | 3 task `FE-RWI-254` s.d. `256`. Ruang ID dipindai global 2026-10-16: terpakai sampai `FE-RWI-253`; berikutnya `FE-RWI-257` |
| roadmap pasangan | [Backend](backend-roadmap-transfer-pasien-masuk.md) · [Traceability](requirement-traceability-transfer-pasien-masuk.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-RWI-241 [BE] ────┬─> FE-RWI-254 ─┬─> FE-RWI-255
                    │               │
BE-RWI-243 [BE] ────┘               └─> FE-RWI-256
                                    │
BE-RWI-242 [BE] ────────────────────┘
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-transfer-pasien-masuk.md](backend-roadmap-transfer-pasien-masuk.md). Garis selalu prasyarat → task.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 | `BE-RWI-241` [BE], `BE-RWI-243` [BE] | `FE-RWI-254` | Banner serah terima klinis & alert status PPRI di Detail Episode |
| 2 | `FE-RWI-254`, `BE-RWI-242` [BE] | `FE-RWI-255` | Laci serah terima klinis dengan PPJA wajib & rekonsiliasi obat |
| 2 | `FE-RWI-254` | `FE-RWI-256` | Verifikasi cetak gelang ranap resmi & serah terima berkas PPRI |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-254` | Banner serah terima klinis aktif dan alert non-blocking kelengkapan berkas PPRI pada Detail Episode Rawat Inap | FR-TRF-002, FR-TRF-007 | 0.14.0 draft; lock TRF-DEL-20261016-01 | `InpatientEpisodeDetailView`, `TransferHandoverBanner`, `InformationAlert` | Pasang banner saat handover Sent; alert info kuning saat dokumen PPRI belum selesai | `BE-RWI-241` [BE], `BE-RWI-243` [BE] | TRF-AC-008, TRF-AC-009 | V-FE + UAT-TRF-01, 03 | UX perawat bangsal; konsistensi alert banner | D-FE |
| `FE-RWI-255` | Laci serah terima klinis (`TransferHandoverDrawer`) dengan area rekonsiliasi obat/alergi dan form pemilihan PPJA wajib | FR-TRF-003, FR-TRF-004, FR-TRF-005 | 0.14.0 draft | `TransferHandoverDrawer`, `BaseSelect`, `ConfirmModal` | Render badge alergi, tabel rekonsiliasi obat, dropdown PPJA wajib, disabled Terima Pasien bila PPJA kosong, dialog alasan tolak | `FE-RWI-254`, `BE-RWI-242` [BE] | TRF-AC-010, TRF-AC-011, TRF-AC-012 | V-FE + UAT-TRF-01, 02 | Validasi keselamatan pasien (PPJA); penanganan error 400/422 | D-FE |
| `FE-RWI-256` | Verifikasi dan penyelarasan aksi pratinjau & cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di Workspace PPRI | FR-TRF-006 | 0.14.0 draft | `AdmissionWorkspaceClient`, `WristbandAndLabelView`, `HandoverChecklistForm` | Verifikasi pratinjau cetak gelang ranap & label stiker; aktifkan tanda tangan perawat ruangan sesudah bed occupied | `FE-RWI-254` | TRF-AC-013, TRF-AC-014 | V-FE + UAT-TRF-01 | Kepatuhan SOP identifikasi pasien & serah terima berkas | D-FE |

---

## Batas Handoff dan Definition of Done

### D-FE — Bukti Wajib Setiap Task Frontend

Task frontend dikerjakan menggunakan konvensi Next.js App Router, JavaScript/JSX, Redux, Axios `InstanceAxios`, serta design token dan base component Quilvian (`@/components/features/base-features/`).

V-FE adalah bundel bukti frontend:
1. `npm run lint` lulus tanpa error baru.
2. `npm run build` lulus.
3. Review kesesuaian komponen visual dan responsivitas.
4. Laporan tracked disimpan di `task/report/frontend/<TASK-ID>.md`.

---

## Kartu Task

### FE-RWI-254 — Banner Serah Terima Klinis Aktif dan Alert Non-Blocking Berkas PPRI pada Detail Episode Rawat Inap

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Detail Episode Rawat Inap menampilkan banner serah terima aktif saat ada pasien transfer masuk dan alert info kuning yang menyatakan berkas PPRI sedang diproses tanpa menghalangi perawatan klinis |
| Ukuran / Batas | Integrasi komponen banner dan alert di `InpatientEpisodeDetailView.jsx` |
| Jejak | FR-TRF-002, FR-TRF-007; TRF-AC-008, TRF-AC-009 |
| Desain / Kontrak | Frontend 17.1; Kontrak Spesifikasi Transfer Bagian 5.A |
| Reuse / Disposisi | `InpatientEpisodeDetailView` (Extend), `TransferHandoverBanner` (Reuse), `InformationAlert` (Reuse) |
| Cakupan | Baca status handover dan `admissionDocumentsSummary` pada detail episode. Jika ada handover `Sent`, render `TransferHandoverBanner`. Jika `admissionDocumentsSummary.isAllCompleted == false`, render alert kuning info non-blocking. Hubungkan tombol banner ke state drawer |
| Batas Source | `src/components/view/health-services/inpatient-management/InpatientEpisodeDetailView.jsx`; `src/components/features/health-services/clinical-management/transfer-handover/TransferHandoverBanner.jsx` |
| Dependency Langsung | `BE-RWI-241` [BE], `BE-RWI-243` [BE] |
| Verifikasi yang Wajib | V-FE (`npm run lint`, `npm run build`) + uji tampilan banner & alert |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-254.md` |

**Acceptance Kriteria Positif**:
- Jika ada transfer handover berstatus `Sent`, banner amber tampil di bagian atas Detail Episode dengan tombol `[Lihat Detail Serah Terima]`.
- Jika dokumen admisi PPRI belum lengkap, alert kuning menampilkan: *"Dokumen admisi di Workspace PPRI sedang diproses ({CompletedCount}/{TotalCount} selesai). Pelayanan dan perawatan medis di bangsal tetap berjalan penuh."*
- Mengklik tombol banner membuka Laci Serah Terima Klinis (`TransferHandoverDrawer`).

**Acceptance Kriteria Negatif**:
- Jika tidak ada handover `Sent`, banner serah terima tidak tampil.
- Jika dokumen PPRI sudah 100% lengkap, alert kuning dokumen PPRI otomatis hilang.

---

### FE-RWI-255 — Laci Serah Terima Klinis dengan Area Rekonsiliasi Obat/Alergi dan Form Pemilihan PPJA Wajib

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Perawat bangsal dapat memverifikasi TTV, alat, riwayat alergi, merekonsiliasi obat dari unit asal, dan wajib memilih perawat PPJA sebelum menerima pasien |
| Ukuran / Batas | Pembaruan komponen `TransferHandoverDrawer.jsx` dan integrasi API accept/reject |
| Jejak | FR-TRF-003, FR-TRF-004, FR-TRF-005; TRF-AC-010, TRF-AC-011, TRF-AC-012 |
| Desain / Kontrak | Frontend 17.1; Kontrak Spesifikasi Transfer Bagian 5.A; Kamus Data 23.1, 23.2 |
| Reuse / Disposisi | `TransferHandoverDrawer` (Extend), `BaseSelect` (Reuse), `BaseButton` (Reuse) |
| Cakupan | Render badge riwayat alergi. Render tabel terapi obat IGD/Poli dengan pilihan Lanjut/Tunda/Stop. Render dropdown PPJA ruangan. Guard tombol `[Terima Pasien]` disabled saat PPJA kosong. Panggil `PATCH /transfer-handovers/{id}/accept` dengan payload lengkap. Tampilkan modal alasan saat klik `[Tolak / Minta Klarifikasi]` |
| Batas Source | `src/components/features/health-services/clinical-management/transfer-handover/TransferHandoverDrawer.jsx`; service/hook terkait |
| Dependency Langsung | `FE-RWI-254`, `BE-RWI-242` [BE] |
| Verifikasi yang Wajib | V-FE + uji fungsi form PPJA, rekonsiliasi obat, dan submit penerimaan |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-255.md` |

**Acceptance Kriteria Positif**:
- Perawat bangsal melihat rincian TTV, alat medis, dan tabel terapi obat dari unit asal.
- Memilih nama perawat PPJA mengaktifkan tombol `[Terima Pasien]`.
- Mengklik `[Terima Pasien]` mengirim request ke server, menutup drawer, dan menghilangkan banner serah terima.

**Acceptance Kriteria Negatif**:
- Tombol `[Terima Pasien]` berstatus disabled jika dropdown PPJA belum dipilih.
- Jika server merespons error `CLI-TRH-002` (akun login sama dengan pengirim), tampilkan toast/alert: *"Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim."*
- Mengklik tolak tanpa alasan min. 10 karakter menampilkan validasi form merah.

---

### FE-RWI-256 — Verifikasi dan Penyelarasan Aksi Pratinjau & Cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di Workspace PPRI

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Petugas PPRI dapat mencetak Gelang Ranap Resmi dan label medis untuk dipasangkan ke pasien, serta checklist serah terima berkas 15 butir dapat diselesaikan bertahap |
| Ukuran / Batas | Verifikasi dan penyelarasan tampilan di `AdmissionWorkspaceClient.jsx` |
| Jejak | FR-TRF-006; TRF-AC-013, TRF-AC-014 |
| Desain / Kontrak | Frontend 17.2; Kontrak Spesifikasi Transfer Bagian 5.B |
| Reuse / Disposisi | `AdmissionWorkspaceClient` (Reuse), `WristbandAndLabelView` (Reuse/Verify), `HandoverChecklistForm` (Reuse/Verify) |
| Cakupan | Pastikan pratinjau visual gelang memuat No RM, Nama, Bed, Kamar, dan Barcode. Pastikan tombol cetak gelang dan label stiker memicu print dialog. Pastikan checklist 15 butir serah terima admisi dapat ditandatangani perawat ruangan saat pasien sudah menempati bed |
| Batas Source | `src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/wristband/`; `src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/handover/` |
| Dependency Langsung | `FE-RWI-254` |
| Verifikasi yang Wajib | V-FE + verifikasi visual pratinjau dan fungsi print |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-256.md` |

**Acceptance Kriteria Positif**:
- Gelang ranap resmi memuat No RM, Nama Pasien, Tgl Lahir, Bed, dan Barcode/QR.
- Tombol cetak gelang memicu pencetakan thermal format gelang pasien.
- Formulir serah terima berkas 15 butir dapat ditandatangani secara elektronik oleh Admisi, CRO, dan Perawat Ruangan.

**Acceptance Kriteria Negatif**:
- Tanda tangan perawat ruangan pada checklist PPRI terkunci jika pasien belum berstatus menempati bed (`Occupied`).
