# Laporan Perubahan Frontend — `FE-IGD-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-044` |
| Judul | Sikap pesanan kepergian ditetapkan dari tab Transfer |
| Slice | `IGD-S05` · `EPIC IGD-07` (sikap pesanan, `MVP-5`) — sisi layar; penutupan susulan `EPIC IGD-13` (`MVP-8`) |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.13.2 |
| Trace | Sisi layar `FR-IGD-045`…`048`, `050`, `051`, `088`; `IGD-DEC-204`, `205`, `206`; aturan sikap `IGD-DEC-078`, `100`…`103`; `IGD-EV-109`; `IGD-FACT-053`, `054`, `056` |
| Contract version | API `0.14.0` §2 (`order-items`) dan §2.3; validation `0.13.0` §5 aturan 1–10, 14 dan §5.1; state `0.9.0` §6a.2 — `approved` (`IGD-DEC-108`, `IGD-DEC-209`). Nol endpoint baru |
| Wewenang UI | Letak daftar, rupa, urutan kolom, dan bentuk isian sikap `DEV_DISCRETION`. Mengikat: (a) keterangan aturan 4 dan 5 terbaca; (b) unit penerima *Handover* = unit tujuan kepergian, tidak dapat diganti; (c) *Cancel* menuntut alasan; (d) nol aksi pada kepergian yang dibatalkan; (e) baris tergantikan tetap terbaca dan bertanda tidak berlaku; (f) ID pengguna mentah tidak ditampilkan |
| Dependency | `FE-IGD-043` 🟡 (source selesai 5 Oktober 2026, berkas yang sama); `BE-IGD-041` 🟡 (source dan build 6 Oktober 2026; 15 dari 17 acceptance terbukti) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (4 berkas), logika 1, kontrak API 1 (memakai kontrak yang ada), database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — izin pemilik 6 Oktober 2026 (*"mulai aja lanjutkan pengerjaan FE-IGD-044"*). Backend baca-saja, kecuali laporan ini dan baris status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `2a985f6d5` (`RizkiV2`), working tree bersih sebelum task (sesudah perubahan CSS penguji dikembalikan, `IGD-DEC-214`) |
| Commit backend yang dijadikan rujukan | `3811fa06` (`rizkiG`) + working tree `BE-IGD-041`, `BE-IGD-064` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore): 13 dari 13 acceptance** (`IGD-DEC-218`, `IGD-DEC-219`; bagian 9). Acceptance 1–8 dan 10–11 terbukti pada uji layar putaran bersama di hasil build dengan akun peran nyata (`044-U1`…`U9`, `041-S11c`); acceptance 9 terbukti di layar untuk modal aksi kepergian dan lewat source untuk modal sikap pesanan (`044-U10` `NOT RUN`, diserahkan ke tim UAT). Bukti mentah dan log backend diperiksa agent; diterima dengan penyimpangan tercatat. Rilis bersama `BE-IGD-041` milik pemilik. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — implementasi selesai 6 Oktober 2026.** Empat berkas (+354/−5); `eslint` 0 error 0 warning; uji IGD lama 91/91. Acceptance 1–11 terpetakan ke source; 12 dan 13 terbukti — `npm run build` lulus 11.12–11.13 WIB (470/470 halaman, nol warning) sesudah port 3000 kosong. **Belum:** uji layar acceptance 1–11 pada putaran bersama `BE-IGD-041`, `BE-IGD-064`, `FE-IGD-043` (`IGD-DEC-208`). Tanpa UAT |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Frontend tidak pernah memanggil `order-items`; pesanan kepergian tidak terlihat dan sikapnya tidak dapat ditetapkan dari layar | `IGD-FACT-053`; pencarian `order-items` di `src` nihil sebelum task |
| *Ajukan Serah Terima* membentuk baris pesanan tanpa sikap lalu menolak `400` *"Masih ada {n} pesanan yang belum ditentukan sikapnya."* — petugas tidak punya jalan menyelesaikannya | `IGD-FACT-054`; `EmergencyDepartureService.SubmitHandoverAsync` |
| Sejak `BE-IGD-041`, pesanan tanpa sikap menahan penutupan kunjungan — tanpa layar ini, kunjungan tertahan tidak dapat dibereskan petugas | `IGD-DEC-204` |
| `GET /emergency-departures/{id}/order-items` mengurutkan baris menurut `ActionAt`; baris tanpa sikap ber-`ActionAt` tanggal minimum | `EmergencyDepartureController.GetOrderItems`; `BuatPesananBelumBersikap` |
| Respons daftar kepergian juga membawa `orderItems`, tetapi kartu mengikat acceptance 1 ke `GET /{id}/order-items` | `EmergencyDepartureResponse.OrderItems` |
| Komponen tab di modul ini menerima state lewat `sections` dari hook; tidak ada yang memakai `useSelector` | `use-emergency-assessment-detail.jsx` |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat IGD yang ditugaskan pada simpul IGD (kewenangan unit asal, diperiksa server).

1. Perawat membuka Ruang Kerja → tab *Transfer Pasien* → segmen *Riwayat*.
2. Setiap kartu kepergian kini memuat bagian **Pesanan saat pasien pergi**: keterangan aturan 4 (penunjang belum dihitung
   otomatis) dan, bila ada pesanan laboratorium, aturan 5 (sikapnya ditetapkan petugas); lalu daftar pesanan berisi uraian,
   jenis, asal, sikap atau *Belum ada sikap*, status penerimaan, waktu sikap, unit penerima, alasan sikap, dan alasan
   penolakan.
3. Pesanan tanpa sikap menawarkan tiga tombol: *Continue*, *Handover*, *Cancel*.
4. Modal konfirmasi menjelaskan akibat pilihan. *Handover* menampilkan unit penerima — unit tujuan kepergian, tanpa pilihan
   lain. *Cancel* menuntut alasan; tombol *Simpan Sikap* nonaktif sampai alasan terisi.
5. Berhasil → modal tertutup, daftar pesanan kepergian itu dimuat ulang, dan kartu pasien dimuat ulang (status kunjungan
   ikut berubah bila sikap itu penahan terakhir).
6. Pesanan yang ditolak unit penerima menampilkan alasan penolakannya dan tombol *Sikap pengganti: …*; sesudah tersimpan,
   baris lama bertanda *Tidak berlaku* dan baris pengganti tampil.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Kepergian dibatalkan | Pesanan tetap terbaca tanpa tombol; keterangan *"Kepergian ini dibatalkan, sehingga pesanannya tidak memerlukan sikap dan tidak menahan penutupan kunjungan."* |
| Kepergian tanpa pesanan | *"Tidak ada pesanan yang tercatat pada kepergian ini."* |
| Gagal memuat pesanan | Kotak merah berisi pesan server dan tombol *Coba lagi* |
| Server menolak sikap (`400`, `403`, `409`) | Modal tetap terbuka; pesan server tampil apa adanya; alasan yang diketik tetap utuh |
| *Ajukan Serah Terima* ditolak karena pesanan tanpa sikap | Pesan tampil di modal (`FE-IGD-043`), dan daftar pesanan kepergian itu dimuat ulang sehingga baris yang baru terbentuk langsung terlihat |

*Contoh.* Ny. Sari (data samaran) naik ke Rawat Inap Melati. Perawat menekan *Ajukan Serah Terima* → modal menampilkan
*"Masih ada 2 pesanan yang belum ditentukan sikapnya."* Di kartu kepergian terbaca *Resep R-0012 — Sikap: Belum ada sikap*
dan *Darah lengkap — Sikap: Belum ada sikap*, dengan keterangan bahwa sikap pesanan laboratorium ditetapkan petugas.
Perawat menekan *Handover* untuk resep — modal menulis *"Unit penerima: Rawat Inap Melati — mengikuti unit tujuan
kepergian dan tidak dapat diganti."* — lalu *Continue* untuk Darah lengkap. *Ajukan Serah Terima* kemudian berhasil.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-IGD-044` (roadmap frontend R3.13.2); validation `0.13.0` §5 aturan 1–14
- Backend baca-saja: `EmergencyDepartureController` (`GET`/`PATCH` `order-items`), `EmergencyDepartureDtos.cs`
  (`EmergencyHandoverOrderItemInput`, `SetOrderItemActionRequest`, `EmergencyHandoverOrderItemResponse`),
  `EmergencyDepartureService` (`ValidateOrderItem`, `SetOrderActionAsync`, `TerapkanAction`, `BuatPesananBelumBersikap`,
  `ToResponse`), enum `EmergencyOrderKind`, `EmergencyOrderSource`, `EmergencyOrderAcceptanceStatus`
- Frontend: `emergency-assessment-transfer-tab.jsx`, `emergency-assessment-slice.jsx`, `emergency-assessment-constant.jsx`,
  `emergency-assessment-detail-view.jsx`, `use-emergency-assessment-detail.jsx`; `base-button.jsx`, `confirm-modal.jsx`,
  `information-alert.jsx`, `status-badge.jsx`; `emergency-assessment.module.css` (kelas yang dipakai ulang)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | +58. `DEPARTURE_ORDER_ACTION`, label dan uraian sikap, status penerimaan beserta label dan varian lencana, label jenis dan asal pesanan, serta `DEPARTURE_ORDER_NOTES` (keterangan aturan 4, aturan 5, kepergian dibatalkan, daftar kosong) |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | +102/−2. Thunk `fetchDepartureOrderItems` (`GET …/{id}/order-items`) dan `setDepartureOrderAction` (`PATCH …/{id}/order-items/{itemId}/action` dengan `{ item }`), mengikuti pola `runDepartureAction`. Bagian `transfers` kini dibentuk `emptyTransferSection()` dan memuat `orderItems` per kepergian beserta `orderSaving`; reducer daftar `transfers` hanya menyentuh `items`, sehingga ruas baru aman |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx` | +1. `onVisitChanged={refreshVisit}` diteruskan ke tab Transfer, sama dengan tab Observasi dan Tindak Lanjut (acceptance 11) |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx` | +193/−3. Fungsi bantu tingkat modul `hasOrderAction`, `orderActionsFor`, `orderNoteFor`, `orderConfirmMessageFor`; pemuatan pesanan setiap kali daftar kepergian dimuat; bagian *Pesanan saat pasien pergi* per kartu kepergian; modal sikap kedua; muat ulang pesanan bila aksi kepergian ditolak |

Akhiran baris keempat berkas tetap CRLF (0 LF tunggal, 0 CR tunggal). Nol baris komentar baru; komentar lama tidak
disunting. Nol route, menu, CSS, komponen baru, `globals.css`, atau backend.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: view → slice (thunk) → `InstanceAxios`. View tidak memanggil Axios.
- State tetap mengalir lewat `sections.transfers` dari hook; nol `useSelector` di komponen, nol perubahan hook.
- Konstanta domain di berkas konstanta modul; fungsi bantu murni ditaruh di tingkat modul view, mengikuti `actionsFor`
  yang sudah ada di berkas yang sama.

**Gerbang base component.**

```
UI GATE: 6 elemen — REUSE 4, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Daftar pesanan per kepergian | `recordList`/`recordItem`/`recordGrid` | Pola kartu kepergian di berkas yang sama | REUSE | Dipakai apa adanya |
| Lencana sikap, penerimaan, berlaku | `Badge` react-bootstrap | Lencana Fisik/Dokumen dan Berlaku/Tidak berlaku pada kartu yang sama | REUSE | Konsisten dengan lencana di sebelahnya |
| Tombol sikap | `BaseButton` `size="sm"` | `base-button.jsx`; dipakai detail view dan tabel Observasi IGD | REUSE | Aksi baru memakai `BaseButton` |
| Keterangan aturan 4 dan 5 | `InformationAlert` `variant="info"` | `information-alert.jsx` | REUSE | — |
| Modal sikap + alasan *Cancel* + unit penerima + galat | `ConfirmModal` + `InformationAlert` | Pola `FE-IGD-042`/`043` | COMPOSE | Dirangkai di view |
| Keadaan memuat/gagal/kosong per kepergian | teks `mutedText` + `InformationAlert` + `BaseButton` | Kelas dan komponen yang ada | COMPOSE | Dirangkai di view |

Pilihan untuk dua baris COMPOSE: **A. dirangkai di view — rekomendasi, dijalankan** (nol komponen baru, risiko regresi ke
modul lain nol); **B. komponen anak baru** khusus daftar pesanan (lebih rapi untuk berkas besar, tetapi menambah berkas
yang tidak diminta UI gate). Catatan konsistensi: tombol aksi kepergian yang sudah ada memakai `<button>` mentah bergaya
`primaryMiniButton`; tombol sikap baru memakai `BaseButton` sesuai checklist C, sehingga rupanya bisa sedikit berbeda.
`StatusBadge` tidak dipakai karena kunci statusnya (`active`, `pending`, …) tidak memetakan sikap pesanan, dan lencana di
sebelahnya sudah memakai `Badge`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Memuat pesanan kepergian..."* sampai daftar pertama tiba; saat dimuat ulang, daftar lama tetap tampil |
| Kosong | *"Tidak ada pesanan yang tercatat pada kepergian ini."* |
| Gagal | `InformationAlert` merah berisi pesan server, tombol *Coba lagi* |
| Tanpa hak akses | Penolakan `403` baca tampil sebagai pesan server di tempat daftar; penolakan `403` sikap (kewenangan unit asal) tampil di modal |
| Menyimpan | Tombol *Simpan Sikap* berputar dan nonaktif; tombol sikap lain nonaktif selama permintaan berjalan |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/api/v1/health-services/emergency-installation-management/emergency-departures`.

#### Health Services / Emergency Installation Management / Emergency Departure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/order-items` | Daftar pesanan satu kepergian, termasuk baris yang sudah tidak berlaku | `EmergencyDeparture : Read` |
| `PATCH` | `/{id}/order-items/{itemId}/action` | Menetapkan sikap atau sikap pengganti. Badan: `{ "item": { orderKind, orderSource, orderReferenceId, externalReference, orderDescription, action, actionReason, toServiceUnitId } }` | `EmergencyDeparture : Update` |

Contoh badan *Handover*: `{ "item": { "orderKind": 1, "orderSource": 1, "orderReferenceId": "<id resep>", "externalReference": null, "orderDescription": "Resep R-0012", "action": 2, "actionReason": null, "toServiceUnitId": "<unit tujuan kepergian>" } }`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node_modules/.bin/eslint` empat berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah, exit 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs` | 91 lulus, 0 gagal | `PASS` | Keluaran perintah |
| `git diff --stat` | 4 berkas, +354/−5 | `PASS` | Keluaran perintah |
| Baris tambahan berisi komentar | Nol | `PASS` | `git diff -U0` |
| Akhiran baris | Keempat berkas CRLF murni | `PASS` | Hitungan byte lewat Node |
| Grep anti-regresi baris tambahan | `<button` 0, `<table` 0, utilitas tipografi 0, `!important` 0, inline style 0, warna literal 0, `actionByUserId`/`acceptedByUserId` 0 | `PASS` | `git diff -U0` |
| `Get-NetTCPConnection -LocalPort 3000` | Pemeriksaan pertama: dipakai `node .next/standalone/server.js` (PID 22620) untuk uji ulang pemilik — build ditunda. Pemeriksaan kedua, sesudah server berhenti: kosong | — | Keluaran perintah |
| `npm run build` | Lulus 11.12.13–11.13.41 WIB: kompilasi 49 detik, 470/470 halaman, `postbuild` standalone siap; nol baris *warning*/*error* pada log | `PASS` | Log build di scratchpad sesi |
| Uji layar acceptance 1–11 | Putaran bersama `IGD-DEC-208` | `NOT RUN` | — |

```
AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (91/91)
AUTOMATED TEST: SKIPPED (opsional) — perubahan berupa komposisi view, thunk, dan konstanta; pemilik melarang unit test frontend baru
MANUAL TEST: NOT RUN — uji layar pada putaran bersama dengan panduan agent (IGD-DEC-208)
```

**Skenario untuk panduan putaran bersama** (hasil build, 1440 × 900, langkah perawat dengan `dimas.kurniawan@rsmmc.local`
— `IGD-DEC-215`; data lewat API atau layar, tidak lewat SQL):

| No | Langkah | Harapan | Acceptance |
| ---: | --- | --- | --- |
| `044-U1` | Kepergian berpesanan resep dan laboratorium; *Ajukan Serah Terima* | Modal menampilkan penolakan `400`; daftar pesanan muncul dengan *Belum ada sikap*, keterangan aturan 4 dan 5, waktu sikap `-` | 1, 8, 9 |
| `044-U2` | Kepergian tanpa pesanan | Kalimat kosong, bukan daftar kosong | 2 |
| `044-U3` | *Continue* pada satu pesanan; rekam jaringan | `PATCH …/action` membawa ruas pesanan baris itu; daftar dimuat ulang | 3 |
| `044-U4` | *Cancel* tanpa lalu dengan alasan | *Simpan Sikap* nonaktif sampai alasan terisi; alasan terkirim sebagai `actionReason` | 4 |
| `044-U5` | *Handover*; rekam jaringan | Unit penerima = unit tujuan kepergian; nol permintaan ke `master-data/service-units` saat aksi | 5 |
| `044-U6` | Unit penerima menolak pesanan (`PENERIMA`); sikap pengganti | Alasan penolakan terbaca; baris lama *Tidak berlaku*, baris pengganti tampil | 6 |
| `044-U7` | Kepergian dibatalkan berpesanan | Pesanan terbaca tanpa tombol; keterangan kepergian dibatalkan | 7 |
| `044-U8` | Sesudah semua bersikap, *Ajukan Serah Terima* | Berhasil | 10 |
| `044-U9` | Kunjungan menunggu penutupan karena pesanan; sikap terakhir dari layar | Kunjungan tertutup; kartu pasien *Selesai* tanpa memuat ulang halaman | 11 |
| `044-U10` | Simulasi penolakan server (misalnya akun tanpa penugasan simpul IGD bila tersedia) | Pesan server di modal; isian utuh | 9 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar pesanan per kepergian dari `GET /{id}/order-items` dengan uraian, jenis, asal, sikap atau *belum ada sikap*, status penerimaan, alasan penolakan, waktu sikap (`-` untuk tanpa sikap) | **Terpenuhi** — `044-U1`; asal dan status penerimaan `044-U5`; alasan penolakan dan waktu sikap `-` `044-U6` (bagian 9) | `fetchDepartureOrderItems`; `renderOrders` (`transfer-tab` baris 277–342); waktu `decided ? formatDateTime(...) : "-"` (baris 322) |
| 2 | Kepergian tanpa pesanan dinyatakan dengan kalimat | **Terpenuhi** — `044-U2` (bagian 9) | `DEPARTURE_ORDER_NOTES.EMPTY` (baris 296) |
| 3 | Tanpa sikap menawarkan *Continue*, *Handover*, *Cancel*; `PATCH …/action` membawa ruas pesanan; daftar dimuat ulang | **Terpenuhi** — tombol `044-U1`; badan `{ "item": … }` dan muat ulang `044-U3` (bagian 9) | `orderActionsFor` (baris 99); `runOrderAction` (baris 226–253) |
| 4 | *Cancel*: simpan nonaktif sampai alasan terisi; alasan → `actionReason` | **Terpenuhi** — `044-U4` (bagian 9) | `requireReason` pada modal (baris 515); `actionReason: action.requiresReason ? reason : null` |
| 5 | *Handover*: unit penerima = unit tujuan kepergian, tidak dapat diganti; nol permintaan `service-units` | **Terpenuhi** — `044-U5` (bagian 9) | `toServiceUnitId: … departure.toServiceUnitId` (baris 243); teks unit di modal (baris 524); tanpa daftar pilihan unit |
| 6 | Pesanan ditolak: alasan terbaca, sikap pengganti tersedia; baris lama *Tidak berlaku*, baris pengganti tampil | **Terpenuhi** — `044-U6`; tombol pengganti *Handover*/*Cancel* lewat source (bagian 9) | `orderActionsFor` untuk `Rejected`; lencana `Berlaku`/`Tidak berlaku`; muat ulang daftar sesudah simpan |
| 7 | Bersikap dan tidak ditolak, serta seluruh pesanan kepergian dibatalkan: tanpa aksi | **Terpenuhi** — `044-U7`, `044-U3`, `044-U8`, `044-U6` (bagian 9) | `orderActionsFor` mengembalikan daftar kosong untuk kepergian `Cancelled`, baris tidak berlaku, dan baris bersikap yang tidak ditolak |
| 8 | Keterangan aturan 4 dan 5 terbaca tanpa kursor | **Terpenuhi** — `044-U1`; kalimat laboratorium pada PNG `044-U1`, `044-U8` (bagian 9) | `orderNoteFor` (baris 113) lewat `InformationAlert` teks biasa |
| 9 | Penolakan server tampil di tempat aksi; isian utuh | **Terpenuhi** — modal aksi kepergian di layar (`043-U1`…`U4`); modal sikap pesanan lewat source, `044-U10` `NOT RUN` diserahkan ke tim UAT (`IGD-DEC-219`, bagian 9) | `setOrderError(result.payload?.message …)`; modal tidak ditutup saat gagal; alasan di `ConfirmModal` baru dikosongkan pada `onExited` |
| 10 | Sesudah sikap seluruh pesanan ditetapkan, *Ajukan Serah Terima* berhasil | **Terpenuhi** — `044-U8` (bagian 9) | Perilaku server; daftar pesanan dimuat ulang sesudah setiap aksi |
| 11 | Satu siklus bersama `BE-IGD-041`: sikap terakhir menutup kunjungan; kartu pasien *Selesai* tanpa memuat ulang | **Terpenuhi** — `044-U9` dan `041-S11c` (bagian 9) | `onVisitChanged?.()` sesudah sikap tersimpan (baris 249); `refreshVisit` diteruskan detail view (baris 304) |
| 12 | Diff: nol route, menu, `globals.css`, backend; nol komentar baru; komentar lama utuh; akhiran baris dipertahankan | **Terpenuhi** | Bagian 6 |
| 13 | `eslint` 0 error; uji IGD lama lulus; tanpa unit test baru; `npm run build` lulus | **Terpenuhi** | Bagian 6 |

**DoD:** laporan tracked ✅; register, node grafik R3.13.2 dan ringkasan, serta traceability ditandai ✅; uji layar satu
siklus dengan `BE-IGD-041` pada hasil build ✅ (6 Oktober 2026 sore, bagian 9). Rilis bersama `BE-IGD-041` — syarat rilis
milik pemilik, belum dijalankan (bagian 9). *Sebelumnya: belum — uji layar satu siklus dan rilis bersama.*

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil pada `eslint` |
| Masalah yang diketahui | (1) Aksi kepergian yang berhasil (misalnya *Catat Tiba* yang memicu penutupan susulan) belum memuat ulang kartu pasien — hanya sikap pesanan yang memanggil `onVisitChanged`, sesuai cakupan acceptance 11. (2) Galat aksi kepergian tetap ikut mengisi `transfers.saveError` bersama (catatan `FE-IGD-043`). (3) Terima/tolak pesanan oleh unit penerima tetap tanpa layar (`FR-IGD-049`, sisa `IGD-EV-109`). (4) Nama pelaku sikap tidak ditampilkan karena respons hanya memuat `actionByUserId`. (5) Syarat klinisi berwenang untuk *Cancel* belum ditegakkan backend; layar tidak menambah penjagaan sendiri |
| Dependency backend | `BE-IGD-041` 🟡 — dirilis bersama (`IGD-DEC-204`); sisa acceptance 14 dan 15 kartu itu diuji pada putaran yang sama |
| Perubahan sampingan | `NONE`. Catatan: git memberi peringatan bahwa `emergency-assessment-observation-tab.jsx` di working copy kini berakhiran LF; berkas itu tidak disentuh task ini dan isinya tidak berbeda menurut git |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M` `emergency-assessment-transfer-tab.jsx`, `emergency-assessment-detail-view.jsx`, `emergency-assessment-constant.jsx`, `emergency-assessment-slice.jsx`. Tidak di-*stage*, tidak di-commit |
| Langkah berikutnya | Panduan uji agent untuk putaran bersama — bagian 6 laporan ini, `FE-IGD-043` acceptance 1–7, `BE-IGD-064` acceptance 1–4, sisa `BE-IGD-041` acceptance 14 dan 15 |

---

## 9. Pemeriksaan bukti uji putaran bersama — 6 Oktober 2026 (sore)

Uji layar dijalankan agen Antigravity atas nama pemilik dengan panduan agent
[`2026-10-06-panduan-uji-putaran-bersama.md`](../../testing/2026-10-06-panduan-uji-putaran-bersama.md) (Blok B, Blok C
bagian layar, Blok D; aturan A1–A18). Agent memutus dari bukti mentah
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-bersama-20261006/` (JSON per skenario, PNG viewport, skrip per
versi) dicocokkan dengan log backend `Logs/quilvian-backend-20261006.json`, bukan dari ringkasan
[laporan penguji](../../testing/2026-10-06-laporan-uji-putaran-bersama.md). Keputusan pemilik: `IGD-DEC-218` dan
`IGD-DEC-219`; fakta `IGD-FACT-073`…`079` di decision log.

**Lingkungan.** Layar dilayani `node .next/standalone/server.js` (proses sejak 11.32.45 WIB) dari `BUILD_ID` 11.13.16 WIB
— build kartu ini, lebih baru dari keempat berkasnya (10.39–10.42 WIB); viewport 1440 × 900; `nextjsPortalNull` benar di
setiap skenario layar. Backend dari DLL 09.31.28 WIB yang memuat `BE-IGD-041` dan `BE-IGD-064`. Seluruh langkah layar
memakai akun `PERAWAT` (`dimas.kurniawan`, `IGD-DEC-215`; `isSuperAdmin: false`, penugasan HR di simpul Instalasi Gawat
Darurat). Kepergian dan pesanan dibentuk lewat API dengan akun peran nyata — `LOKET` untuk encounter, `PERAWAT` untuk
kunjungan dan kepergian, `DOKTER` untuk tindak lanjut, `PENERIMA` untuk kedatangan dan terima/tolak — tanpa SQL tulis.
Nol permintaan SuperAdmin dan nol permintaan ke `/api/v1/administrator/**` sejak 04.20 UTC.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `044-U1` | **Terbukti** — acceptance 1, 3 (tombol), 8 | Kartu VB1 memuat *Pesanan saat pasien pergi* dan keterangan *Pemeriksaan penunjang belum dapat dihitung otomatis…*; tiga baris *Uji PB B1-OBAT/LAB/TINDAKAN* masing-masing *Belum ada sikap*, jenis *Obat*/*Laboratorium*/*Tindakan*, tiga tombol *Continue*/*Handover*/*Cancel*; tidak ada tanggal `0001`. `GET …/order-items` `200` 05.18.46 UTC. Kalimat laboratorium terbaca sebagai teks biasa pada PNG `044-U1` dan `044-U8` |
| `044-U3` | **Terbukti** — acceptance 3 | *Continue* pada B1-LAB → `PATCH …/action` `200` 05.18.54 UTC, badan `{ "item": { "orderKind": 3, "orderSource": 2, "orderReferenceId": null, "externalReference": "UJI-PB-B1-LAB", "orderDescription": "Uji PB B1-LAB", "action": 1, "actionReason": null, "toServiceUnitId": null } }`; modal tertutup; disusul `GET …/order-items` dan `GET` kunjungan; baris *Sikap: Continue* tanpa tombol |
| `044-U4` | **Terbukti** — acceptance 4 | *Simpan Sikap* `disabled` sebelum alasan diisi; badan `action` 9, `actionReason` `Uji 044 batal`; `200`; baris *Sikap: Cancel* dan *Alasan sikap: Uji 044 batal* (PNG `044-U5`) |
| `044-U5` | **Terbukti** — acceptance 1 (asal, status penerimaan), 5 | Modal memuat *"Unit penerima: Rawat Inap"* dan *"mengikuti unit tujuan kepergian dan tidak dapat diganti."*, tanpa pilihan unit; badan `action` 2, `toServiceUnitId` = unit tujuan kepergian; `200`; nol permintaan `…/master-data/service-units` sejak tombol ditekan; baris *Sikap: Handover*, *Penerimaan: Menunggu penerimaan*. PNG: ketiga baris memuat asal *Luar sistem · UJI-PB-B1-…* dan waktu sikap |
| `044-U8` | **Terbukti** — acceptance 7, 10 | *Ajukan Serah Terima* → `POST …/submit-handover` `200` 05.19.03 UTC, `handoverStatus` 2; modal tertutup sendiri; disusul `GET …/emergency-departures` dan `GET …/order-items`; kartu *Dokumen: Menunggu Unit Tujuan*; ketiga baris tanpa tombol sikap |
| `044-U2` | **Terbukti** — acceptance 2 | Kartu VB2 memuat *Pesanan saat pasien pergi* dan *"Tidak ada pesanan yang tercatat pada kepergian ini."* |
| `044-U7` | **Terbukti** — acceptance 7 | Kartu VB3 (*Fisik: Dibatalkan*): baris *Uji PB B3-OBAT* *Belum ada sikap*, keterangan *"Kepergian ini dibatalkan, sehingga pesanannya tidak memerlukan sikap…"*, nol tombol sikap |
| `044-U6` | **Terbukti** — acceptance 1 (alasan penolakan), 6 | Baris *Uji PB C3-TOLAK* memuat *Alasan penolakan: Uji PB tolak* dan tombol *Sikap pengganti: Continue*; disimpan → `PATCH …/action` `200`, respons `supersedesOrderItemId` = baris lama; PNG: baris lama *Penerimaan: Ditolak Unit Tujuan* · *Tidak Berlaku* tanpa tombol, baris pengganti *Sikap: Continue* · *Berlaku*. PNG yang sama memperlihatkan baris *Uji PB C3-KOSONG* tanpa sikap dengan waktu sikap `-` |
| `044-U9` | **Terbukti** — acceptance 11 | Tanpa memuat ulang halaman: *Continue* pada C3-KOSONG → `PATCH …/action` `200` 05.19.59 UTC, disusul `GET /emergency-visits/{VC3}` `200` 05.20.00 UTC dari layar; lencana kartu pasien (`visitStatusBadge` di `section[aria-label="Informasi Pasien IGD"]`) terbaca *Selesai*. API `041-S11c`: `visitStatus` 9, `UpdateBy` = `PERAWAT`, encounter 9 |
| `044-U10` | **NOT RUN** — acceptance 9 kaki modal sikap | Akun pembanding perawat tanpa penugasan IGD tidak disediakan (Blok D opsional) |

Pencocokan: seluruh permintaan di `jaringan[]` cocok dengan log (method, path, kode status, akun); putusan dihitung ulang
dari `pemeriksaan` — sembilan `PASS`, satu `NOT RUN`.

**Acceptance 9 (`IGD-DEC-219`).** Penolakan server di tempat aksi terbukti di layar untuk modal aksi kepergian pada
kartu yang sama (`043-U1`…`U4`: pesan server di kotak merah, modal tetap terbuka, alasan utuh). Modal sikap pesanan memakai
state `orderError` dan `ConfirmModal` sendiri (`emergency-assessment-transfer-tab.jsx` baris 508–528) dengan pola yang
sama — modal ditutup hanya bila permintaan berhasil, `setOrderError(result.payload?.message …)` sebaliknya — tetapi jalur
itu tidak dijalankan di layar karena `044-U10` `NOT RUN`. Dinyatakan lewat source atas keputusan pemilik (preseden
`IGD-DEC-201`); kejadiannya diserahkan ke tim UAT.

**Penyimpangan — bukti diterima dengan penyimpangan tercatat (`IGD-DEC-218`).**

| # | Penyimpangan | Aturan | Penanganan |
| ---: | --- | --- | --- |
| a | Persiapan, satu sesi layar 04.47–04.50 UTC, dan lingkup percobaan 1–2 tidak dilaporkan lengkap; bukti percobaan 1–2 tertimpa. Percobaan 2 sudah menjalankan Blok B pada VB1 percobaan 1 (tiga sikap dan *Ajukan* `200`); percobaan 3 memakai VB1 baru | A11, A12 | Dicatat (`IGD-FACT-076`) |
| b | Pemeriksaan skrip lebih longgar dari panduan: `044-U1` tidak memeriksa kalimat laboratorium, asal, dan waktu sikap `-`; `044-U6` tidak memeriksa tombol pengganti *Handover* dan *Cancel*; PNG `044-U9` dan `044-U7` tidak memperlihatkan kartu pasien dan baris B3-OBAT | A6, A9 | Dinilai agent dari PNG lain dan DOM (tabel di atas); tombol pengganti *Handover*/*Cancel* lewat source (`orderActionsFor` untuk `Rejected`) |
| c | Lencana dicocokkan tanpa membedakan huruf besar karena `.badge` global mengkapitalkan teks (*Belum ada sikap* terbaca *Belum Ada Sikap*) — penyebab enam `FAIL` percobaan 2 | A7 | Dicatat; bukan cacat layar — teks source sesuai kalimat panduan |
| d | Daftar pasien bersih ditulis tetap di skrip; kueri pemilihnya tidak tersimpan | A10 | Dicatat |

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore)**: 13 dari 13 acceptance — 1–8 dan 10–11 terbukti di layar, 9 terbukti di layar untuk modal aksi kepergian dan lewat source untuk modal sikap pesanan (`IGD-DEC-219`), 12–13 bagian 6 |
| Implementation | Selesai — empat berkas (+354/−5), tidak berubah sejak build 11.13 WIB |
| Developer verification | `eslint` 0/0, uji IGD lama 91/91, `npm run build` lulus (bagian 6); uji layar Blok B dan C pada hasil build, bukti mentah dan log diperiksa agent |
| Manual test | Dijalankan agen penguji pada hasil build dengan akun peran nyata (tabel di atas); bukan pengganti UAT |
| UAT | Belum dijalankan — diserahkan ke tim UAT, termasuk `044-U10`; bukan klaim UAT |
| Dependency backend | `BE-IGD-041` ✅ (6 Oktober 2026 sore, `IGD-DEC-218`); `BE-IGD-064` ✅ (`IGD-DEC-216`) |
| Butir DoD rilis | *Rilis bersama `BE-IGD-041`* adalah syarat rilis milik pemilik — commit, push, dan deploy bukan wewenang agent. Kedua kartu kini ✅, sehingga syarat itu dapat dipenuhi dalam satu rilis |
| Komentar lama yang basi | Nihil |
| Masalah yang diketahui | Tetap seperti bagian 8. Tambahan dari putaran ini: perawat IGD ditolak `403` untuk `GET …/master-data/service-units` saat Ruang Kerja dimuat (`ServiceUnit : Read`, konfigurasi peran — catatan `FE-IGD-043` butir 5); tidak memengaruhi modal *Handover*, yang memang tidak memakai daftar unit |
| Langkah berikutnya | Pemilik: commit, lalu rilis `FE-IGD-044` bersama `BE-IGD-041` (`IGD-DEC-204`) |
