# Laporan Perubahan Frontend — `FE-IGD-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-040` |
| Judul | Panel waktu tiba pada Detail triage |
| Slice | `S3` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.12 |
| Trace | `FR-IGD-078` (sisi layar); `IGD-DEC-147`, `152`, `159`, `160`; `AT-IGD-175` (sisi layar); `03-frontend-architecture.md` §13.3 D, §13.5 |
| Contract version | API `0.11.0` §8.3.4 (`PATCH /{id}/arrival-time` dan tiga ruas baru `EmergencyVisitResponse`); validation `0.8.0` §10.4 — `approved` (`IGD-DEC-157`) |
| Wewenang UI | `DEV_DISCRETION` untuk letak panel (03 §13.6); memakai isian tanggal-jam dan kotak peringatan yang sudah ada; nol elemen `NEW` |
| Dependency | `BE-IGD-058` ✅ (build pemilik dan uji API S1–S9 terbukti, 1 Oktober 2026) |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 2 (9), logika 1, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — wewenang pemilik 1 Oktober 2026. Backend baca-saja, kecuali laporan ini dan status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`) + working tree `FE-IGD-038`, `036`, `039` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `b9076c71` + working tree `BE-IGD-053`, `057`, `058`, `059` |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — Implementation Complete.** Sembilan berkas (tujuh source diubah, satu panel baru, satu test); `eslint` 0 error; unit test berkas terkait 79/79. **Belum:** `npm run build` dan uji layar U1–U7 (milik pemilik) |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Respons kunjungan sudah membawa sumber waktu tiba, nama pengonfirmasi, dan waktu konfirmasi | `EmergencyVisitResponse.ArrivalTimeSource`, `ArrivalConfirmedByName`, `ArrivalConfirmedAt` (`BE-IGD-055`) — ikut pada daftar `GET /emergency-visits` yang dipakai Detail triage |
| Layar tidak menampilkan satu pun dari ketiganya | `emergency-triage-form-view.jsx`, `emergency-triage-patient-summary.jsx` |
| Endpoint konfirmasi sudah ada dan teruji | `PATCH /emergency-visits/{id}/arrival-time` (`BE-IGD-058` ✅) |
| Detail triage punya dua wujud | Formulir (kunjungan belum ditriage) dan Riwayat Triage (status `Triaged` ke atas, termasuk pasien Tangani Segera) |

Akibatnya waktu tiba sementara — waktu terdaftar yang dipakai Tangani Segera — tidak terlihat sebagai sementara dan
tidak dapat dikonfirmasi dari layar.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat triage, pada Detail triage seorang pasien.

1. Bila waktu tiba belum dikonfirmasi, panel kuning tampil di bawah ringkasan pasien:
   - **Waktu tiba sementara** — kunjungan lahir lewat Tangani Segera; waktunya waktu terdaftar.
   - **Waktu tiba belum dikonfirmasi** — kunjungan lama yang dibuat sebelum alur baru.
2. Isian waktu tiba sudah terisi nilai yang tercatat. Perawat membiarkannya (mengonfirmasi) atau mengoreksinya, lalu
   menekan **Konfirmasi**.
3. Panel berubah menjadi satu baris hijau: *"Tiba 09.28, 01 Okt 2026 · dikonfirmasi Ns. Ani"*.
4. Pada formulir triage, menekan **Simpan Pemeriksaan** selagi waktu tiba belum dikonfirmasi tidak menyimpan apa
   pun; layar meminta konfirmasi waktu tiba lebih dulu.

*Contoh.* Pasien kejang ditangani segera pukul 09.35. Di Detail triage panel kuning berbunyi *"Waktu tiba sementara —
Waktu tiba tercatat 09.35, 01 Okt 2026 (waktu terdaftar)"*. Perawat mengisi 09.28 dan menekan Konfirmasi; panel
menjadi *"Tiba 09.28 … · dikonfirmasi Ns. Ani"*. Bila ia mengisi 10.00, pesan server *"Waktu tiba tidak boleh lebih
lambat dari mulai penanganan pukul 09.35 WIB tanggal 01-10-2026."* tampil di bawah isian dan nilainya tidak berubah.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Waktu di masa depan | `400` dari server tampil di bawah isian |
| Waktu lebih lambat dari peristiwa klinis pertama | `409` dari server tampil apa adanya, menyebut peristiwa dan jamnya |
| Isian dikosongkan | Tombol Konfirmasi tidak aktif |
| Respons tidak membawa sumber waktu tiba | Panel tidak tampil dan penyimpanan triage tidak ditahan |
| Tanpa izin `EmergencyVisit : Update` | Pesan penolakan server tampil di bawah isian |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-IGD-040`; API §8.3.4; validation §10.4; 03 §13.3 D, §13.5; laporan `BE-IGD-058`.
Source: `emergency-management-triage.service.js` (sumber konteks pasien), `information-alert.jsx`,
`emergency-triage-follow-up-section.jsx`, `emergency-triage.module.css`, dan seluruh berkas pada 3.2.
Backend (baca saja): `EmergencyVisitDtos.cs`, pemetaan respons pada `EmergencyVisitController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-management-triage-constant.jsx` | + `ARRIVAL_TIME_SOURCE` (0, 1, 2), label panel, pesan |
| `src/utils/…/emergency-management-triage-utils.jsx` | + `resolveArrivalTimeSource`, `isArrivalTimeConfirmed`, `requiresArrivalConfirmation`, `resolveArrivalTimeLabel`, `buildArrivalPendingMessage`, `buildArrivalConfirmedSummary` |
| `src/lib/state/slice/…/emergency-management-triage-slice.jsx` | + thunk `confirmEmergencyArrivalTime` (`PATCH …/arrival-time`), cabang state `arrivalTime`; saat berhasil, empat ruas waktu tiba pada pasien terpilih diperbarui dari respons |
| `src/lib/hooks/…/use-emergency-management-triage-form.jsx` | + `confirmArrivalTime`; `submit` menahan penyimpanan selagi sumber `0`/`1` dan mengembalikan kalimat permintaannya |
| `src/components/view/…/components/emergency-triage-arrival-time-panel.jsx` (**baru**) | Panel waktu tiba: kuning beserta isian dan tombol untuk sumber `0`/`1`; satu baris hijau untuk sumber `2` |
| `src/components/view/…/emergency-triage-form-view.jsx` | Panel dipasang di bawah ringkasan pasien pada formulir, dan diteruskan ke Riwayat Triage; kalimat permintaan konfirmasi di atas tombol simpan |
| `src/components/view/…/components/emergency-triage-history-panel.jsx` | Menerima isi tambahan (`children`) di bawah judul — tempat panel waktu tiba |
| `tests/unit/emergency-triage-utils.test.mjs` | + enam kasus `FE-IGD-040` |

Nol baris komentar baru. **Tidak disentuh:** CSS mana pun, route, backend.

### 3.3 Kepatuhan arsitektur frontend

View → hook → slice → util/constant; permintaan lewat `InstanceAxios` di thunk; aturan sumber waktu tiba berupa
fungsi murni di `src/utils`.

`UI GATE: 4 elemen — REUSE 3, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kotak peringatan dan baris ringkas | `InformationAlert` (`warning`, `success`) | `base-features/information-alert.jsx` | `REUSE` | — |
| Isian tanggal-jam | `Form.Control type="datetime-local"` | Dialog Mulai Triage (`FE-IGD-036`), `emergency-assessment-soap-tab.jsx` | `REUSE` | — |
| Tombol Konfirmasi | `Button` react-bootstrap berkelas `primaryButton` | `emergency-triage-history-panel.jsx`, dialog retriage | `REUSE` | — |
| Panel waktu tiba | Rangkaian ketiganya di dalam `formSection` | Kartu bagian pada formulir triage | `COMPOSE` | Lihat keputusan di bawah |

> **Keputusan: letak dan bentuk panel** (`DEV_DISCRETION`)
>
> - **A. Kartu di bawah ringkasan pasien, pada formulir maupun Riwayat Triage — dijalankan.** Terlihat sebelum
>   perawat mulai mengisi; pasien Tangani Segera, yang Detail triage-nya berupa riwayat, tetap mendapat panelnya.
> - **B. Hanya pada formulir.** Lebih sedikit sentuhan, tetapi waktu tiba sementara — kasus utamanya — justru tidak
>   pernah mendapat panel, karena pasien Tangani Segera tidak pernah melihat formulir.
> - **C. Dialog saat menekan Simpan.** Tidak menambah wilayah layar, tetapi keadaan "masih sementara" tidak terlihat
>   sampai perawat menekan simpan.

Grep anti-regresi pada berkas baru dan baris yang ditambah: nol `<button>`, nol `<table>`, nol utilitas tipografi
Bootstrap, nol stylesheet berubah.

### 3.4 Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Penahanan simpan hanya pada **formulir** triage; *Nilai Ulang Pasien* pada Riwayat Triage tidak ditahan | Kartu menyebut "menyimpan triage". Penilaian ulang dilakukan pada pasien yang kondisinya berubah; menahannya demi isian administratif berisiko menunda penilaian. Panelnya tetap tampil di layar itu. Bila pemilik menghendaki penilaian ulang ikut ditahan, perubahannya satu pemeriksaan pada tombolnya |
| Sumber yang tidak dikirim atau tidak dikenal: panel tidak tampil, simpan tidak ditahan | Layar tidak menebak; `IGD-DEC-160` — backend memang tidak menolak triage karena hal ini |
| Isian terisi awal dari `arrivalDateTime` kunjungan, dibulatkan ke menit | 03 §13.4: nilai tidak pernah dari jam browser. Isian tanggal-jam berketelitian menit, jadi mengonfirmasi tanpa mengubah menyimpan nilai pada menit yang sama dengan detik nol — selalu sama atau lebih awal, tidak pernah lebih lambat |
| Tanpa pemeriksaan "masa depan" di layar | Sama dengan `FE-IGD-036`: jam browser tidak dipakai sebagai pembanding |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol *"Menyimpan..."*; isian dan tombol nonaktif |
| Kosong | `NOT APPLICABLE` — panel selalu punya nilai waktu tiba dari kunjungan; bila sumbernya tidak dikirim, panel tidak tampil |
| Gagal | Pesan server di bawah isian; waktu tiba tidak berubah |
| Tanpa hak akses | Pesan penolakan server di bawah isian |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}/arrival-time` | Mengonfirmasi atau mengoreksi waktu tiba: `{ "arrivalDateTime" }` | `EmergencyVisit : Update` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits` | Konteks pasien Detail triage — kini ruas `arrivalTimeSource`, `arrivalConfirmedByName`, `arrivalConfirmedAt` ikut dibaca | `EmergencyVisit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` sembilan berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `node --test` empat berkas IGD terkait | 79 dari 79 lulus | `PASS` | Termasuk enam kasus `FE-IGD-040` |
| Suite unit penuh | 2175 dari 2181 lulus; 6 gagal | `UNRELATED EXISTING ISSUE` | Enam yang sama dengan laporan `FE-IGD-038`: Hemodialisa (4), menu Setup Bank Darah (1), petty cash (1) |
| Baris komentar pada tambahan | Nol | `PASS` | Pencarian pada `git diff` dan berkas baru |
| Akhiran baris | CRLF dipertahankan | `PASS` | Pemeriksaan byte |
| `npm run build` | — | `NOT RUN` | Milik pemilik |
| Uji layar U1–U7 | — | `NOT RUN` | Milik pemilik |

`AUTOMATED TEST: npm run test:unit — FAIL (6 kegagalan di modul lain; 79/79 berkas terkait lulus)`

`MANUAL TEST: NOT FEASIBLE — backend tidak berjalan pada sesi ini dan uji layar dijalankan pemilik`

Uji manual: `REQUIRED`.

### 6.1 Skenario uji layar untuk pemilik

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| U1 | Pasien Tangani Segera (sumber `1`): buka Detail triage dari daftar (*Lihat Riwayat*) | Panel kuning **Waktu tiba sementara**, kalimat menyebut "(waktu terdaftar)", isian terisi nilai tercatat | 1 |
| U2 | Kunjungan lama (sumber `0`) yang masih Menunggu Triage: buka *Isi Triage* | Panel kuning **Waktu tiba belum dikonfirmasi** di bawah ringkasan pasien | 1 |
| U3 | Dari U1: mundurkan beberapa menit, tekan Konfirmasi | `PATCH …/arrival-time` `200`; panel menjadi baris hijau *"Tiba … · dikonfirmasi <nama>"* — nama, bukan GUID | 2 |
| U4 | Pasien Tangani Segera lain: isi waktu **sesudah** mulai penanganan, tekan Konfirmasi | `409` dengan kalimat server yang menyebut peristiwa dan jamnya tampil di bawah isian; panel tetap kuning | 3 |
| U5 | Dari U2: isi formulir triage, tekan Simpan Pemeriksaan **tanpa** mengonfirmasi | Nol permintaan simpan; kalimat *"Konfirmasi waktu tiba lebih dulu…"* tampil di atas tombol | 4 |
| U6 | Dari U5: tekan Konfirmasi, lalu Simpan Pemeriksaan | Panel hijau; penyimpanan triage berjalan seperti biasa | 2, 4 |
| U7 | Pasien yang lahir lewat Mulai Triage (sumber `2`): buka *Isi Triage* | Hanya baris hijau ringkas; simpan tidak ditahan | 1 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Panel tampil untuk sumber `0`/`1` dengan label yang benar; ringkas untuk `2` dengan nama pengonfirmasi | **Terpenuhi pada source** | Panel + unit test `FE-IGD-040 K1` (lima kasus). Uji layar U1, U2, U7 belum |
| 2 | Konfirmasi sah → panel berubah menjadi ringkas | **Terpenuhi pada source** | Reducer memperbarui pasien terpilih dari respons; panel dipasang ulang menurut sumbernya. Uji layar U3, U6 belum |
| 3 | Koreksi sesudah peristiwa klinis pertama → pesan backend tampil; waktu tiba tidak berubah | **Terpenuhi pada source** | Thunk meneruskan pesan server; state pasien hanya berubah saat berhasil. Uji layar U4 belum |
| 4 | Simpan triage pada sumber `0`/`1` meminta konfirmasi lebih dulu | **Terpenuhi pada source** — untuk formulir triage | `submit` pada hook; unit test `FE-IGD-040 K4`. Penilaian ulang tidak ditahan (3.4). Uji layar U5 belum |
| 5 | `eslint` 0 error; unit test lulus; nol CSS baru; build dan uji layar — milik pemilik | **Sebagian** | `eslint` 0 error; 79/79; nol CSS; build dan uji layar belum |

DoD: laporan tracked ✅ (berkas ini).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kewajiban konfirmasi hanya ditegakkan layar (`IGD-DEC-160`); klien lain tetap dapat menyimpan triage tanpa konfirmasi |
| Masalah yang diketahui | (1) Penilaian ulang tidak ditahan (3.4). (2) Pasien Tangani Segera membuka Detail triage sebagai Riwayat Triage yang kosong dan tanpa tombol pengisian triage pertama — keadaan yang sudah ada sebelum task ini; panel waktu tiba tetap tampil di sana. (3) Celah layar untuk melengkapi keluhan, cara datang, dan jenis kasus sesudah Tangani Segera tetap seperti dicatat kartu (menunggu amendment desain layar) |
| Dependency backend | `BE-IGD-058` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sembilan berkas oleh task ini (delapan diubah, satu baru), beberapa di antaranya juga memuat perubahan `FE-IGD-036` dan `039`. Tanpa stage, commit, atau push |
| Langkah berikutnya | R3.14: `BE-IGD-061`, `062`, `063`, lalu `FE-IGD-042`, `041` |
