# Laporan Task — `FE-LAB-18` Penjaga Tanggal pada Penyaring Laboratorium

| Field | Value |
|---|---|
| `task_id` | `FE-LAB-18` |
| `blueprint_id` | `LAB-BP-001` |
| Tanggal | 2026-09-16 |
| Status | **`SELESAI`** |
| Requirement | `FR-10.4` |
| Keputusan | `LAB-DEC-064`, `LAB-DEC-071`, `LAB-DEC-074` |
| Acceptance criteria | `AC-98`, `AC-99` |
| Kontrak | **Nol perubahan** |
| Migration | **Nol** |
| Frontend SHA saat mulai | `686038858` |
| Asal task | **Audit kesiapan `LAB-RDY-001`, kondisi `C-02`** — bukan requirement baru |

---

## 1. Kenapa task ini ada

Task ini **tidak lahir dari permintaan fitur**. Ia lahir dari audit kesiapan yang menemukan
cacat pada layar yang **sudah dipakai petugas sejak `FE-LAB-09`**: penyaring tanggal pada tiga
menu Pemeriksaan menerima tanggal sampai **20 tahun ke depan** dan rentang terbalik, keduanya
tanpa penolakan apa pun. Akibatnya bukan galat, melainkan sesuatu yang lebih buruk — **daftar
kosong yang tidak menjelaskan sebabnya**, yang bagi petugas terlihat seperti "datanya hilang".

`LAB-DEC-064` dan `LAB-DEC-071` sebenarnya sudah melarang keduanya, tetapi lahir dalam konteks
`BR-50` (Menu Hasil, slice `S17`). `LAB-DEC-074` menetapkan keduanya **mengikat seluruh penyaring
tanggal Laboratorium**, dengan alasan yang bukan kerapian dokumen: dua layar Laboratorium yang
berperilaku berbeda untuk penyaring berbentuk sama akan dilaporkan petugas sebagai bug.

---

## 2. Yang Dikerjakan

| Berkas | Perubahan |
|---|---|
| `src/components/features/base-features/filter-date-picker.jsx` | Prop **opsional** `max`; `maxValue` ternormalisasi; penjaga `isBeyondMax`; penegakan pada `selectDate`, `selectToday`, tombol hari, dan tombol `Hari ini` |
| `src/style/components/features/base-features/filter-date-picker.module.css` | Kelas `.dayBlocked` — teredam, bukan disembunyikan |
| `src/lib/hooks/health-services/laboratory-management/lab-monitoring-rules.js` | `todayDateValue()` dan `validateDateRange()`, keduanya murni |
| `src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx` | `maxDateValue` dan `dateRangeError`; efek pengambilan data berhenti saat rentang tidak sah |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx` | `max` diteruskan ke kedua pemilih tanggal; alert `warning` berisi sebab penolakan |
| `tests/unit/lab-monitoring-rules.test.mjs` | **+8 pemeriksaan** |
| `tests/unit/filter-date-picker-max-guard.test.mjs` | **Baru, +6 pemeriksaan** |
| `tests/e2e/lab-monitoring-date-guard.spec.mjs` | **Baru, +2 pemeriksaan layar** |

---

## 3. Tiga Keputusan Teknis yang Pantas Dibaca Ulang

### 3.1 Prop opsional, bukan aturan yang dipaksakan dari komponen base

`FilterDatePicker` dipakai **133 berkas** lintas modul. Menanam "tolak tanggal masa depan" di
dalam komponennya akan mengunci setiap penyaring tanggal di seluruh aplikasi — termasuk layar
yang justru **butuh** tanggal masa depan, seperti penjadwalan.

Karena itu `max` **opsional dan bawaannya kosong**. Aturan Laboratorium ditegakkan oleh
**pemanggilnya**, bukan oleh komponen bersama.

### 3.2 Perbandingan teks, bukan `Date`

Bentuk `YYYY-MM-DD` berpadding tetap, sehingga urutan leksikografisnya sama dengan urutan
kronologisnya. Membandingkan sebagai teks menghindari zona waktu dan jam ikut terseret.

### 3.3 `toISOString()` sengaja TIDAK dipakai

`todayDateValue()` menyusun tanggal dari komponen **lokal**. `toISOString()` mengubah ke UTC
lebih dulu, sehingga bagi pengguna WIB ia mengembalikan tanggal **kemarin** pada pukul 00:00
sampai 07:00 — **persis jam ketika petugas laboratorium shift pagi mulai bekerja**. Cacat itu
hanya muncul di pagi hari dan akan sangat sulit dilacak. Ada satu pemeriksaan khusus untuknya.

---

## 4. Bukti

### 4.1 Uji unit — 999/999 lulus

```
node --import ./tests/helpers/register.mjs --test tests/unit/
# tests 999
# pass 999
# fail 0
```

Seluruh berkas uji dijalankan, **bukan hanya milik Laboratorium** — komponen yang disentuh
dipakai bersama, sehingga cakupan pembuktiannya pun harus lintas modul.

### 4.2 Build produksi — lulus

```
npm run build
…
[prepare-standalone] Standalone runtime siap dijalankan.
[exited with code 0]
```

### 4.3 Bukti layar — aplikasi yang benar-benar berjalan

```
npx playwright test tests/e2e/lab-monitoring-date-guard.spec.mjs
2 passed (6.1s)
```

Dijalankan terhadap `.next/standalone` pada `127.0.0.1:3710`, dengan jawaban API dipasang
lewat `page.route` — sehingga **nol bergantung pada backend** maupun basis data.

| Pemeriksaan | Yang dibuktikan |
|---|---|
| `AC-98` | Kalender penyaring: sel **hari ini aktif**, sel **hari sesudahnya nonaktif**. Bukan sekadar aturannya benar — aturannya benar-benar **terpasang** pada layar |
| `AC-99` | Rentang terbalik memunculkan kalimat sebabnya, **dan nol permintaan tambahan berangkat** selama rentangnya masih terbalik |

**`AC-99` diukur dengan menghitung permintaan yang tiba, bukan dengan melihat tampilan.**
Satu-satunya cara membuktikan sesuatu **tidak dikirim** adalah menghitung yang sampai:
jumlahnya diambil sebelum rentang dibalik, lalu dibandingkan sesudah jeda dua detik.

> **Dua kekeliruan saya saat menulis spec ini, dicatat supaya tidak terulang.** Pertama,
> pemicu `FilterDatePicker` adalah `button` ber-nama-aksesibel, **bukan** `input`
> ber-`placeholder` — `getByPlaceholder` tidak akan pernah menemukannya. Kedua, penyaring
> layar ini **tidak sinkron dari URL**; memasang `?startDate=…` tidak mengubah apa pun.
> Keduanya membuat spec versi pertama gagal, dan keduanya kekeliruan uji — bukan cacat
> produk.

### 4.4 Acceptance criteria

| AC | Pemeriksaan | Hasil |
|---|---|---|
| `AC-98` | Tanggal sesudah hari ini ditolak pada kedua ruas | **Lulus** |
| `AC-98` | **Hari ini sendiri tetap boleh** — batasnya inklusif | **Lulus** |
| `AC-98` | Penjaga ditegakkan pada `selectDate`, bukan hanya atribut `disabled` | **Lulus** |
| `AC-98` | Hari di luar batas tetap tampil, hanya teredam dan menolak klik | **Lulus** |
| `AC-99` | Rentang terbalik ditolak **beserta sebabnya** | **Lulus** |
| `AC-99` | `Tgl Awal` = `Tgl Akhir` tetap sah — pencarian satu hari mungkin | **Lulus** |

### 4.5 Pembuktian terbalik — 132 pemakai lain nol terdampak

Ini asersi terpenting pada task ini, dan sengaja ditulis sebagai uji tersendiri:

| Yang dibuktikan | Cara |
|---|---|
| Bawaan `max` kosong | Asersi atas `max = ""` |
| Tanpa `max`, penjaga **selalu** mati | Asersi atas hubung-singkat `Boolean(maxValue) && …` |
| `max` yang salah tulis **melonggarkan**, bukan mengunci | Asersi atas normalisasi `isSafeDateInputValue(max) ? … : ""` |

Konsekuensinya dapat ditelusuri tanpa menjalankan apa pun: tanpa prop `max`, `maxValue` bernilai
`""`, `isBeyondMax` mengembalikan `false` untuk setiap tanggal, `disabled` selalu `false`, dan
nol kelas tambahan dipasang — **identik dengan perilaku sebelumnya**.

### 4.6 Lint — nol tambahan

| Keadaan | Jumlah |
|---|---|
| `filter-date-picker.jsx` **sebelum** disentuh (versi `HEAD`) | 5 warning, 0 error |
| `filter-date-picker.jsx` **sesudah** | 5 warning, 0 error |

Kelima warning itu **sudah ada sebelumnya** dan seluruhnya tentang `setState` di dalam efek pada
baris yang tidak disentuh task ini. **Sengaja tidak diperbaiki** — ia perbaikan tersendiri pada
komponen milik bersama, dan menyelipkannya ke sini akan memperbesar risiko task yang seharusnya
kecil.

---

## 5. Batas Task Ini

- **Nol perubahan kontrak dan nol migration.** Backend sudah inklusif pada kedua ujung
  (`>= mulai`, `<= sampai`); yang kurang memang penjaga di sisi layar.
- **Penjaga dipasang pada tiga menu Pemeriksaan saja.** Layar Laboratorium lain yang punya
  penyaring tanggal belum ikut; bila kelak dibutuhkan, prop-nya sudah tersedia dan tinggal
  diteruskan.
- **Bukti layar memakai jawaban API yang dipasang uji**, bukan backend sungguhan. Yang
  dibuktikan perilaku layar terhadap penyaring tanggal — bukan kebenaran kueri backend, yang
  memang bukan cakupan task ini dan sudah inklusif pada kedua ujungnya sejak semula.
- **Penjaga dibuktikan pada menu Patologi Klinik.** Ketiga menu Pemeriksaan memakai view,
  hook, dan konstanta penyaring yang **sama persis** — `LAB_MONITORING_FILTER_DEFINITION`
  bahkan objek yang sama, bukan salinan — sehingga membuktikan satu cukup mewakili ketiganya.
