# Laporan Perubahan Frontend — `FE-HRD-339`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-339` |
| Judul | Gerbang uang muka/settlement di layar dan jenis "Perjalanan Dinas" di kotak masuk |
| Slice | `S-D5` amandemen / `MVP-71` — gelombang 2 · `EPIC HRD-123` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 28, kartu `FE-HRD-339` |
| Trace | `FR-TRV-31`, `FR-TRV-34`; `HRD-DEC-226`, `HRD-DEC-227` |
| Contract version | `v28` 34.2, 34.3, 34.5 — approved 19 November 2026 |
| Wewenang UI | Desain frontend 32.2 (layar terdampak) + `DEV_DISCRETION` untuk teks keterangan dan label "Anggaran Disetujui" (lihat bagian 3) |
| Dependency | `FE-HRD-337` ✅, `BE-HRD-378` ✅ |
| Klasifikasi | `STANDARD` — konstanta, tiga view perjalanan dinas, satu modal, tiga view kotak masuk; uji unit + uji browser dengan data asli |
| Task mode | `FRONTEND` (instruksi pengguna 19–20 November 2026: "lanjut") |
| Target tulis | Frontend `QuilvianSystemFrontendDev` (8 berkas diubah, 1 berkas uji baru); laporan, roadmap, dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `564365595` (branch `YogaV2`) + perubahan working tree `FE-HRD-338` dan task ini |
| Commit backend yang dijadikan rujukan | `815ce085` (branch `yoga`). Uji browser memakai biner backend 19 November 15.53 yang sudah memuat `BE-HRD-374` s.d. `380`, tetapi belum memuat merge `QuilvianIntegrationBackend` sesudahnya (lihat bagian 4) |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi, dibuktikan di browser sebagai pemohon (Dewi) dan atasan (dr. Bima) terhadap backend lokal dan devYoga |

---

## 1. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/travel/travel-constants.js` | Ditambah `isTravelApproved` (ruas `isApproved` dari server didahulukan; baris daftar tidak membawanya, jadi dinilai dari status), `canRequestTravelAdvance` (hanya `Approved` yang disetujui), `TRAVEL_AWAITING_SETTLEMENT_STATUSES` (`Approved`, `AdvancePaid`), `canSettleTravel` (disetujui, atau SPPD lama dengan uang muka `Paid`, `HRD-DEC-227`), serta `TRAVEL_WORKFLOW_REFERENCE_TYPE` = `HRD_TRV_REQUEST` dan `TRAVEL_WORKFLOW_LABEL` = "Perjalanan Dinas" |
| `src/components/view/hr/travel/unified-travel-advances-view.jsx` | Pilihan SPPD memakai `canRequestTravelAdvance`; sebelumnya `["Submitted", "Approved"]`. Plafon 80% dari anggaran disetujui saja, tanpa cadangan estimasi (§34.2, `HRD-DEC-226`). Keterangan modal: "Hanya SPPD yang sudah disetujui atasan yang dapat dipilih." |
| `src/components/view/hr/travel/modal/SettlementSubmitModal.jsx` | Pilihan SPPD memakai `canSettleTravel` dengan daftar uang muka `Paid` yang sudah dimuat modal; sebelumnya `Submitted` ikut terpilih |
| `src/components/view/hr/travel/unified-travel-dashboard-view.jsx` | Radar "Menunggu Settlement" memakai `TRAVEL_AWAITING_SETTLEMENT_STATUSES`, jadi SPPD yang menunggu persetujuan tidak lagi terhitung |
| `src/components/view/hr/workflow/approval-inbox-view.jsx` | `getReferenceTypeLabel` mengenal `HRD_TRV_REQUEST` → "Perjalanan Dinas" (rute `/hr/workflow/approval-inbox`) |
| `src/lib/constants/hr/approvals/approval-constants.js` | `REQUEST_TYPE` ditambah `HRD_TRV_REQUEST: "Perjalanan Dinas"` |
| `src/components/view/hr/approvals/unified-approval-inbox-view.jsx` | Kolom "Jenis" dan opsi filter jenis memakai `resolveApprovalInfo(REQUEST_TYPE, …)`; sebelumnya kode mentah (rute menu `/hrd/approvals/inbox`, `/corporate/hrd/approvals/inbox`) |
| `src/components/view/hr/approvals/unified-approval-detail-view.jsx` | Judul, "Jenis Permohonan", dan teks modal aksi memakai label yang sama |
| `tests/unit/business-travel-v28-gates.test.mjs` (baru) | 8 uji: gerbang uang muka, gerbang settlement, radar dasbor, view memakai gerbang bersama dan menampilkan galat aksi, payload formulir tanpa uang muka, label kotak masuk lama dan terpadu |

`unified-travel-create-view.jsx` tidak diubah oleh task ini. AC2 sudah dipenuhi source sebelumnya: formulir hanya punya tiga langkah (Rute & Agenda, Estimasi Biaya, Ringkasan & Simpan), payload `POST` tidak memuat ruas uang muka, dan layar sukses menulis "Uang muka dapat diajukan dari layar Uang Muka sesudah SPPD disetujui." Uji unit mengunci perilaku ini.

**Perluasan cakupan**: kartu menyebut `approval-inbox-view.jsx`. Ternyata menu "Kotak Masuk Persetujuan Terpadu (V2)" memakai `unified-approval-inbox-view.jsx` dan `unified-approval-detail-view.jsx`, keduanya menampilkan kode `requestType` mentah. Kedua view ikut diberi label agar AC3 berlaku di layar yang benar-benar dipakai atasan. Perubahannya hanya pemetaan label lewat peta `REQUEST_TYPE` yang sudah ada.

---

## 2. Verifikasi

### 2.1 Validasi otomatis

| Perintah | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/business-travel-v28-gates.test.mjs tests/unit/business-travel-v28-status.test.mjs` | 14/14 lulus (8 baru + 6 dari `FE-HRD-338`) |
| Kontrol negatif: 6 mutasi (gerbang uang muka kembali `Submitted`/`Approved`; radar memuat `Submitted`; gerbang settlement selalu `true`; label kotak masuk lama dihapus; `REQUEST_TYPE` tanpa `HRD_TRV_REQUEST`; kolom Jenis tanpa `render`) | 6/6 tertangkap (uji gagal), source dipulihkan |
| `npm run lint:errors` | Exit 0 |
| `npm run build` | "Compiled successfully", 890/890 halaman, termasuk `/hrd/travel/advances`, `/hrd/travel/settlements`, `/hrd/approvals/inbox`, `/hr/workflow/approval-inbox` dan padanan `/corporate/...` |

### 2.2 Uji browser (manual terotomasi)

Skrip Playwright di scratchpad menjalankan `next dev` 3000 → backend lokal 7184 → devYoga. Pemohon login sebagai Dewi Anggraeni, atasan sebagai dr. Bima; sandi berasal dari pengguna dan tidak disimpan. Hak yang dibutuhkan diberikan sementara: Dewi 6 izin uang muka/settlement/SPPD; dr. Bima `ApprovalInbox:Read`, `WorkflowInboxInstance:ViewInbox`/`ViewInstanceDetail`, `WorkflowApprovalAction:ReturnForRevision`. Semua hak dikembalikan identik di blok `finally` (Dewi 11 izin, Bima 45 izin). Tidak ada data baru di devYoga.

Data SPPD Dewi saat uji: `0003` Disetujui (belum cair), `0008` Draf, `0009` Menunggu Persetujuan (workflow, belum disetujui), `0010` Settlement Diajukan, sisanya Dibatalkan/Ditolak.

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uang muka → "+ Ajukan Uang Muka" → daftar SPPD | Hanya `SPPD/RSQ/2026/11/0003`; `0009` (menunggu) dan `0008` (Draf) tidak ada; keterangan "Hanya SPPD yang sudah disetujui atasan…" tampil | `PASS` (AC1) | `339-1-uang-muka-opsi.png` |
| Uang muka dipaksa: daftar SPPD dicegat agar `0009` tampil Disetujui, lalu `POST` asli ke server | Server menjawab `409`; pesan "SPPD belum disetujui. Ajukan SPPD dan tunggu persetujuan sebelum mengajukan uang muka." tampil di modal; modal tetap terbuka; satu `POST`; tidak ada uang muka baru | `PASS` (AC1) | `339-2-uang-muka-409.png` |
| Settlement → "+ Ajukan Settlement" → daftar SPPD | Hanya `0003`; `0009` tidak ada | `PASS` | `339-3-settlement-opsi.png` |
| Settlement dipaksa (cara sama, satu butir bernota Rp250.000) | `409`; pesan "SPPD belum disetujui sehingga belum dapat dipertanggungjawabkan." tampil; modal tetap terbuka; tidak ada settlement baru | `PASS` | `339-4-settlement-409.png` |
| Formulir SPPD baru | Tiga langkah, tanpa langkah 4 dan tanpa isian uang muka; tidak ada `POST` yang terkirim | `PASS` (AC2) | Keluaran skrip |
| Kotak masuk lama `/hr/workflow/approval-inbox` (dr. Bima) | Baris `WF-20261119080941-…` (SPPD `0009`) berjenis "Perjalanan Dinas"; tombol Setujui, Revisi, Tolak; kode `HRD_TRV_REQUEST` tidak tampil | `PASS` (AC3) | `339-6e-kotak-masuk-lama.png` |
| Detail kotak masuk terpadu `/hrd/approvals/{id}` | Judul "Perjalanan Dinas oleh Dewi Anggraeni — Keperawatan"; Jenis Permohonan "Perjalanan Dinas" | `PASS` (AC3) | `339-6d-detail.png` |
| Antrean kotak masuk terpadu `/hrd/approvals/inbox` | Data asli: antrean kosong (temuan backend, bagian 4). Dengan `GET workflow/inbox` dan `/summary` ditiru (satu baris SPPD `0009`): kolom Jenis "Perjalanan Dinas", filter "Perjalanan Dinas (1)", tombol Kembalikan | `PASS` (rendering, respons tiruan) | `339-6-kotak-masuk.png`, `339-6f-terpadu-tiruan.png` |
| Tampilan ponsel (390 px) kotak masuk | Terbaca | `PASS` | `339-7-kotak-masuk-ponsel.png` |

Aksi setuju/kembalikan/tolak tidak dikirim di task ini. Aksi yang sama sudah dibuktikan di `FE-HRD-338` (dr. Bima mengembalikan SPPD) dan tidak diubah.

---

## 3. Wewenang UI yang dipakai (`DEV_DISCRETION`)

| Pilihan | Alasan |
| --- | --- |
| Label "Pagu" di modal uang muka menjadi "Anggaran Disetujui", tanpa cadangan ke estimasi | Plafon server dihitung dari anggaran disetujui (§34.2); menampilkan estimasi menyesatkan bila anggaran disetujui kosong |
| Keterangan modal dan teks daftar kosong ("Belum ada SPPD disetujui…") | Menjelaskan mengapa SPPD yang baru diajukan belum muncul |
| SPPD tetap dapat dipaksa lewat API dan galat server ditampilkan apa adanya | Server penentu akhir (`VR-TRV-30`/`31`); layar hanya mencegah pilihan yang pasti ditolak |

---

## 4. Delta kontrak dan temuan

| Temuan | Dampak | Tindak lanjut |
| --- | --- | --- |
| **Antrean Kotak Masuk Terpadu (V2) tidak memuat SPPD.** `ApprovalInboxQueryService.GetInboxAsync` menyaring `AssignmentStatus == "Pending"` dan `ApproverEmployeeId`, sedangkan engine Workflow V2 (`WorkflowService`) membuat penugasan `Available` dengan `ApproverEmployeeId = null` dan `AssignedApproverUserId` terisi. SPPD `0009` milik dr. Bima tidak tampil; detail menyatakan "Tidak ada tahap yang menunggu keputusan Anda" karena mencari tahap `Pending`, padahal tahapnya `InProgress` | Atasan melihat SPPD hanya di kotak masuk lama `/hr/workflow/approval-inbox`. Ini juga berlaku untuk workflow lain yang dibuat `WorkflowService` | Perlu task backend (penyelarasan status/kunci penugasan di kueri kotak masuk V2) atau keputusan pengguna tentang kotak masuk yang dipakai atasan. Tidak dikerjakan di task frontend ini |
| `BusinessTravelResponseDto` (daftar) tidak membawa `isApproved`; hanya detail (§34.4) | Gerbang daftar dinilai dari status (`Approved` dan status sesudahnya). SPPD lama berstatus `Approved` tanpa `ApprovedAt` masih dapat dipilih, lalu ditolak server `409` dengan pesan yang tampil | Dicatat untuk pass blueprint (API 34.4): bila ingin tepat, tambahkan `isApproved` ke DTO daftar |
| Layar uang muka/settlement tidak memakai data contoh (risiko traceability) | Kedua layar sudah membaca API sejak `FE-HRD-R34` | Risiko ditutup, tanpa task baru |
| Biner backend uji belum memuat merge `QuilvianIntegrationBackend` (`815ce085`): `catch … when (HrSafeError.IsDomain(ex))` dan endpoint unggah struk settlement | Jalur yang diuji (gerbang `409` `BE-HRD-378`) tidak berubah oleh merge; unggah struk tidak dipakai uji | `FE-HRD-340` sebaiknya memakai biner hasil build terbaru |
| Galat `409` di modal uang muka berada di bawah isi modal yang dapat digulir | Pesan perlu digulir pada layar pendek | Perilaku tata letak modal yang sudah ada; tidak diubah |

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. SPPD belum disetujui tidak dapat dipilih untuk uang muka; bila dipaksa lewat API, pesan `409` tampil | Terpenuhi | Uji unit + `339-1`, `339-2` |
| 2. Formulir SPPD tidak lagi mengirim uang muka bersamaan dengan SPPD Draf | Terpenuhi (source as-is, dikunci uji unit) | Uji unit + browser |
| 3. Baris workflow SPPD di kotak masuk berlabel "Perjalanan Dinas"; aksi memakai yang sudah ada | Terpenuhi di kotak masuk lama dan detail terpadu dengan data asli; antrean terpadu dengan respons tiruan karena temuan backend | `339-6d`, `339-6e`, `339-6f` |
| Build dan lint | Lulus | Bagian 2.1 |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Antrean kotak masuk terpadu kosong untuk workflow SPPD (bagian 4, butuh backend) |
| Penyegaran blueprint | Desain frontend 32.2 menyebut `approval-inbox-view.jsx` saja; layar menu yang dipakai adalah `unified-approval-inbox-view.jsx` (ikut diubah). API 34.4: `isApproved` hanya di detail |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `FE-HRD-340` (verifikasi browser `AT-HRD-TRV-39` dan `UAT-TRV-06` s.d. `UAT-TRV-12`) |
