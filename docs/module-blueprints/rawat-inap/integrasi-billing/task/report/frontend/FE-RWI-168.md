# Laporan Perubahan Frontend — `FE-RWI-168`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-168` |
| Judul | Gerbang kasir pada Penutupan Episode |
| Slice | `MVP-1` / `RWF-W1` (bagian penutupan) — gelombang eksekusi 2 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-168` |
| Trace | `FR-RWF-005`, `FR-RWF-008`; `RWI-DEC-187`; `AC-RWF-004`, `AC-RWF-008`; `UAT-RWF-03`, `UAT-RWF-12` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.3 `FE-INT-02`; API 3.2 (`closure-readiness`, `close`, `close-with-override`, kode `INP-CLS-010`/`011`/`012`) |
| Wewenang UI | Tata letak dan warna `DEV_DISCRETION`. Mengikat: syarat izin kasir dibaca langsung tiap 10 detik, override hanya dengan permission dan alasan, tanpa PIN |
| Dependency | `FE-RWI-166` ✅ (sesi yang sama); `BE-RWI-153` [BE] 🟡 — source selesai dan build PASS, alur pulang lengkap belum UAT |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang ada (1), hak akses terkait (1), satu workflow (1) |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02`; implementasi ikut commit pengguna `f6eca4ef7`; satu penyempurnaan susulan pada `use-inpatient-closure.jsx` belum di-commit |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI.** Keenam acceptance criteria terpetakan ke source. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026** |

**Otorisasi eksekusi.** Roadmap berstatus `DRAFT`; pengguna memerintahkan seluruh task dikerjakan
sampai selesai pada 5 Oktober 2026.

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti | Akibat |
| --- | --- | --- |
| Penutupan tidak mengirim `ExpectedVersion` | `buildCloseEpisodePayload`, `buildCloseOverridePayload` lama | Backend kini menolak 400 "ExpectedVersion wajib diisi" — penutupan dari layar selalu gagal |
| Tombol override hanya bagi nama peran supervisor dan baru muncul setelah penutupan biasa ditolak | `resolveOverrideAuthority` lama memakai `actor.isSupervisor` dan `hasBeenRejected` | Melanggar `RWI-DEC-187` (hak dari permission); dan karena tombol Tutup kini nonaktif selama syarat belum lengkap, override tidak akan pernah muncul |
| Tombol Tutup selalu aktif | `inpatient-closure-view.jsx` | Kriteria 1 meminta tombol nonaktif selama syarat belum terpenuhi |
| Syarat dibaca sekali saat layar dibuka | `use-inpatient-closure.jsx` | Kontrak meminta penyegaran 10 detik |
| Istilah "menembus gerbang keuangan", "Lunas/Belum Lunas" | view, konstanta, toast | Kartu meminta istilah "tanpa izin kasir" |

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Episode hanya ditutup normal setelah kasir memberi izin; penutupan tanpa izin kasir
tetap mungkin, tetapi hanya bagi pemegang hak dan selalu beralasan.

**Pelaku.** Petugas admisi dan supervisor (`InpatientDischarge : Close`); penutupan tanpa izin kasir
hanya bagi pemegang `InpatientDischarge : CloseOverride`.

**Langkah.**

1. Petugas membuka Penutupan Episode. Lima syarat tampil beserta tanda Sudah/Belum.
2. Syarat ke-4 **Kasir memberi izin** membawa badge status kasir dari Billing, misalnya
   "Menunggu kasir". Daftar syarat dan badge menyegarkan diri tiap 10 detik.
3. Tombol **Tutup Episode** aktif hanya bila kelima syarat terpenuhi.
4. Petugas menekan Tutup Episode. Layar membaca ulang syarat; bila masih siap, dialog konfirmasi
   muncul, lalu penutupan dikirim bersama versi episode yang terbaca layar.
5. Bila yang menahan **hanya** izin kasir dan petugas memegang `CloseOverride`, bagian
   **Tutup Tanpa Izin Kasir** muncul. Petugas menulis alasan yang jelas, lalu menekan
   **Tutup tanpa izin kasir…** dan mengonfirmasi. Tidak ada isian PIN.

**Contoh.** Kasir menyetujui pukul 10.00.00. Paling lambat 10.00.10 syarat ke-4 berbunyi
"Disetujui kasir" dan tombol Tutup Episode aktif.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kasir mencabut izin saat layar terbuka | Dalam 10 detik syarat ke-4 kembali "Belum" dan tombol terkunci |
| Tombol ditekan sebelum layar segar | Pembacaan ulang menghentikan pengiriman; banner merah berisi kalimat syarat dari server ("Episode belum dapat ditutup: kasir belum memberi izin") |
| Kasir mencabut di antara konfirmasi dan pengiriman | Server menjawab 422 `INP-CLS-010` (atau `011` bila Billing tidak terbaca); pesannya tampil sebagai banner merah |
| Alasan override "..." | Ditolak di layar dengan pesan alasan wajib; bila lolos ke server, 400 `INP-CLS-012` tampil apa adanya |
| Versi episode berubah | 409; detail episode dimuat ulang supaya versinya benar |
| Tanpa `CloseOverride` | Bagian penutupan tanpa izin kasir tidak dirender, apa pun nama perannya |

**Hasil.** Penutupan tanpa izin kasir menandai episode "Ditutup tanpa izin kasir", menyimpan alasan
dan status kasir saat itu, serta memunculkan episode di daftar pantau penutupan tanpa izin kasir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientDischargeController.cs`, `InpDischargeService.Closure.cs` (`BuildClosureConditionsAsync`,
`CloseEpisodeInternalAsync`, `CloseWithOverrideAsync`), `InpatientClosureDtos.cs`,
`InpatientEpisodeDtos.cs` (`Version`, `IsClosedWithoutFinancialClearance`); frontend
`use-inpatient-closure.jsx`, `inpatient-closure-view.jsx`, `inpatient-closure-utils.jsx`,
`inpatient-closure-constants.jsx`, `inpatient-episode-utils.jsx`, `use-permission.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-closure-utils.jsx` | `resolveOverrideAuthority` membaca `canCloseOverride`; syarat "sudah ditolak" dicabut; kedua payload membawa `expectedVersion` |
| `src/lib/constants/health-services/inpatient-management/inpatient-closure-constants.jsx` | Istilah "tanpa izin kasir"; pesan alasan; panduan syarat ke-4 menunjuk layar kasir |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | `normalizeEpisodeDetail` membaca `version` dan `isClosedWithoutFinancialClearance` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx` | Penyegaran syarat tiap 10 detik; `usePermission("InpatientDischarge", "CloseOverride")`; versi episode dikirim; 409 memuat ulang detail; banner berisi kalimat syarat server |
| `src/components/view/health-services/inpatient-management/inpatient-closure-view.jsx` | Badge status kasir pada syarat ke-4 dan tab Izin Kasir; tombol Tutup nonaktif selama belum siap; istilah diganti |
| `tests/unit/inpatient-closure.test.mjs` | Tes kriteria 2 dan 3 diperbarui ke kontrak `1.1.0` |

### 3.3 Kepatuhan arsitektur frontend

Logika kewenangan dan payload berada di utils murni; hook mengurus pembacaan berkala dan hak akses;
view hanya merangkai. Struktur Bootstrap bawaan pada view penutupan dipertahankan apa adanya, tidak
direfaktor.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Badge syarat ke-4 | badge `FE-INT-06` | `billing-status-badge.jsx` | REUSE | — |
| Penutupan tanpa izin kasir | bagian dan `ConfirmModal` yang ada | `inpatient-closure-view.jsx` | REUSE | Hak dari permission |

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil kesiapan penutupan episode rawat inap..."; badge "Memeriksa status kasir..." |
| Kosong | "Daftar syarat penutupan tidak dapat dibaca dari server. Muat ulang halaman ini sebelum menutup episode." bila server tidak mengirim syarat |
| Gagal | Banner merah beserta "Coba Lagi"; pembacaan latar yang gagal tidak menghapus daftar syarat terakhir |
| Tanpa hak akses | Halaman ditutup `AccessDeniedGate`; bagian override tidak dirender tanpa `CloseOverride` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/closure-readiness` | Lima syarat; disegarkan tiap 10 detik | `InpatientDischarge : Read` |
| `POST` | `/{episodeId}/close` | Penutupan normal dengan `ExpectedVersion` | `InpatientDischarge : Close` |
| `POST` | `/{episodeId}/close-with-override` | Penutupan tanpa izin kasir dengan alasan | `InpatientDischarge : CloseOverride` |

#### Inpatient Billing Operational

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{episodeId}/billing-status` | Badge syarat ke-4 | `InpatientBillingOperational : Read` |

Kode status: `422` `INP-CLS-010` kasir belum memberi izin; `422` `INP-CLS-011` status kasir tidak
dapat dibaca; `400` `INP-CLS-012` alasan kosong atau hanya tanda baca; `409` versi episode berubah.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya garis dasar di luar task | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Termasuk "tanpa CloseOverride bagiannya tidak dirender", "muatan membawa expectedVersion" |
| ESLint `use-inpatient-closure.jsx` sesudah penyempurnaan terakhir | 0 masalah | `PASS` | Dua warning `preserve-manual-memoization` yang sempat muncul sudah diperbaiki |
| `npm run build` final | `✓ Compiled successfully in 64s`, exit 0 | `PASS` | Mencakup penyempurnaan terakhir |
| Uji manual dua akun (kasir dan admisi): setuju, cabut, override | Tidak dijalankan | `NOT RUN` | Dikecualikan |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — butuh dua akun nyata (kasir dan admisi) serta Billing yang menyetujui dan mencabut izin; tidak tersedia di sesi ini. Dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Izin `PENDING` → tombol Tutup nonaktif dan syarat 4 "Menunggu kasir" | Terpenuhi | `disabled={closeLoading \|\| !isReady \|\| …}`; badge `closure-cashier-status-badge` |
| 2. Kasir menyetujui → dalam 10 detik syarat 4 "Disetujui kasir" dan tombol aktif | Terpenuhi pada source | Interval `CASHIER_STATUS_POLLING_INTERVAL_MS` untuk syarat dan badge |
| 3. Kasir mencabut saat layar terbuka → tombol terkunci; bila ditekan sebelum segar, 422 `INP-CLS-010` sebagai banner | Terpenuhi | Penyegaran 10 detik; pembacaan ulang di `requestClose` menampilkan kalimat syarat yang sama dengan `INP-CLS-010`; 422 dari server tampil sebagai banner merah |
| 4. Override "..." → pesan `INP-CLS-012`; alasan jelas → episode "Ditutup tanpa izin kasir" | Terpenuhi | `validateOverrideForm`; pesan server diteruskan; tanda dari `IsClosedWithoutFinancialClearance` pada kartu `FE-INT-06` |
| 5. Tanpa `CloseOverride` tidak melihat tombol, apa pun nama perannya | Terpenuhi | `resolveOverrideAuthority` tanpa pembacaan peran; tes kriteria 2 |
| 6. Tidak ada isian PIN | Terpenuhi | Payload hanya `reason` dan `expectedVersion` |
| DoD: lint dan build lulus; laporan tracked | Terpenuhi | Bagian 6 |
| DoD: uji manual dua akun | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Panduan "Tindak Lanjut Oleh" syarat ke-4 kini menunjuk layar invoice kasir, bukan layar Kelayakan Keuangan yang sudah menjadi riwayat |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `BE-RWI-153` 🟡 — gerbang "satu jendela rilis" bersama `BE-RWI-146`, `152`, `153` tetap berlaku |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meng-commit dan me-merge branch di tengah pekerjaan |
| Status Git | Implementasi di commit pengguna `f6eca4ef7`; ` M src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx` (penyempurnaan banner sesudah commit) |
| Langkah berikutnya | Uji di peramban dengan akun kasir dan admisi untuk jalur setuju, cabut, dan override |
