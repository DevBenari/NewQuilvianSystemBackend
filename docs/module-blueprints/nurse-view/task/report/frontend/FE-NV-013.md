# Laporan Perubahan Frontend — `FE-NV-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-013` |
| Judul | Bunyi Konsol Panggilan |
| Slice | `MVP-8` — `EPIC NV-08` (gelombang frontend 3, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-013` |
| Trace | `FR-NV-051`; `NV-DEC-045`; `NV-FE-007`, `010`; `G-14`; `NV-CAP-27`; state §7; `03-frontend-architecture.md` R2.6 |
| Contract version | api §7 dan §10, tanpa endpoint baru |
| Wewenang UI | Jeda pengingat 15–60 detik dan lama tunda (`NV-FE-007`); nada Darurat (`NV-FE-010`) |
| Dependency | `FE-NV-012` 🟡 (sesi yang sama) |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 3 (0), logika sedang (1), kontrak yang sudah ada (1), UI kecil (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | `src/lib/hooks/health-services/nurse-view-management/patient-call-console/use-nurse-view-patient-call-sound.jsx` dan view konsol |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7d32e7cec` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `589fb513` + `BE-NV-012`–`014` belum di-commit |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 9 November 2026** — AC 3/3 terpetakan ke source; `npx eslint` 0 error, 0 warning. Belum: rekaman video bersuara, ditunda atas permintaan Rizki |

---

## 1. Keadaan yang ditemukan di awal

Konsol (`FE-NV-012`) belum berbunyi. Aplikasi sudah punya `/audio/queue-dingdong.mp3` dan pola
deteksi bunyi diblokir browser di layar antrean.

## 2. Proses bisnis dari sisi pengguna

Contoh `NV-DEC-045`, dengan jeda pengingat 30 detik sesudah bunyi terakhir:

| Jam | Kejadian | Bunyi |
| --- | --- | --- |
| 10.00.00 | 305-A memanggil | Bunyi pendek (dingdong) |
| 10.00.20 | 331-C Darurat | Nada Darurat: empat nada tinggi-rendah bergantian |
| 10.00.50 | Belum ada klaim | Pengingat (nada Darurat karena masih ada Darurat menunggu) |
| 10.01.10 | 331-C diklaim | — |
| 10.01.20 | 305-A masih menunggu | Pengingat (dingdong) |
| 10.01.30 | 305-A diklaim | Senyap |

**"Matikan bunyi"** menunda pengingat 5 menit dan menampilkan "Pengingat ditunda sampai HH:MM.
Panggilan baru tetap berbunyi."; tombol berganti menjadi "Nyalakan pengingat". Bila browser
menahan bunyi, tampil tombol **Aktifkan suara** beserta penjelasannya; sekali diklik, bunyi berjalan.
TV tetap tanpa suara.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-priority-reservation-list.jsx` (`playArrivalSound`), `use-queue-display-screen.jsx`
(buka kunci audio lewat `AudioContext.resume` dan WAV senyap), konstanta
`QUEUE_VOICE_DINGDONG_PUBLIC_PATH`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/patient-call-console/use-nurse-view-patient-call-sound.jsx` | **Baru.** Selisih daftar (panggilan baru dan kenaikan ke Darurat); dingdong untuk biasa; nada Darurat lewat Web Audio (tanpa aset baru); pengingat 30 detik sesudah bunyi terakhir selama ada `Menunggu`; tunda 5 menit; deteksi `NotAllowedError` → `isBlocked`; `enableSound` membuka kunci audio |
| `…/nurse-view-patient-call-console-view.jsx` | Tombol Aktifkan suara / Matikan bunyi / Nyalakan pengingat di `DataFilter`; info bunyi ditahan dan pengingat ditunda (`InformationAlert`) |
| `…/patient-call-console-utils.jsx` | `diffCallEvents` |

Pembanding bunyi memakai daftar mentah dari server, bukan daftar yang dihitung ulang tiap detik,
dan diatur ulang saat lantai diganti, sehingga ganti lantai tidak membunyikan seluruh panggilan
sebagai "baru".

### 3.3 Kepatuhan arsitektur frontend

Bunyi diatur di hook; view hanya menampilkan tombol dan info. Tanpa berkas audio baru.

**Tabel keputusan base component:**

| Kebutuhan UI | Kandidat base | Status | Rekomendasi |
| --- | --- | --- | --- |
| Tombol Aktifkan suara / Matikan bunyi | `BaseButton` | `REUSE` | Di `actions` `DataFilter` |
| Info bunyi ditahan / pengingat ditunda | `InformationAlert` | `REUSE` | `warning` / `info` |

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| `SuaraDitahanBrowser` | Tombol "Aktifkan suara" + "Browser menahan bunyi. …" |
| `PengingatDitunda` | "Pengingat ditunda sampai HH:MM. Panggilan baru tetap berbunyi." |

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Bunyi bereaksi terhadap daftar `GET /patient-calls/active` yang dimuat
ulang oleh sinyal atau cadangan (`FE-NV-012`).

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` | 0 error, 0 warning |
| Rekaman video bersuara | `NOT RUN` — ditunda atas permintaan Rizki |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest.`
`MANUAL TEST: NOT FEASIBLE — bunyi harus didengar dan direkam Rizki; uji ditunda.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 urutan contoh `NV-DEC-045` | 🟡 Terpetakan ke source | §2; `diffCallEvents`, pemeriksa pengingat tiap detik |
| AC-2 pengingat ditunda, panggilan baru tetap berbunyi | 🟡 Terpetakan ke source | Bunyi panggilan baru tidak memeriksa tunda |
| AC-3 browser baru → "Aktifkan suara"; sesudah klik bunyi berjalan | 🟡 Terpetakan ke source | `NotAllowedError` → `isBlocked`; `enableSound` |
| DoD — lint 0 errors | Terpenuhi | §6 |
| DoD — rekaman bersuara | **Belum** | — |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Kelelahan alarm bila pengingat terlalu rapat (30 detik, di tengah rentang 15–60); kebijakan autoplay tiap browser berbeda |
| Status Git | `??` hook bunyi (berkas konsol lain dicatat di `FE-NV-012`) |
| Langkah berikutnya | Rekaman bersuara oleh Rizki sesudah backend `BE-NV-014` hidup |
