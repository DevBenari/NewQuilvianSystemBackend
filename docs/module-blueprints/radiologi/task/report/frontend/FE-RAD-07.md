# Laporan Perubahan Frontend — `FE-RAD-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-07` |
| Judul | Daftar kerja per alat |
| Epic | `EPIC RAD-07` |
| Requirement | `FR-RAD-060`, `FR-RAD-062`, `FR-RAD-063` (penomoran blueprint) |
| Decision | `RAD-DEC-012`, `RAD-DEC-013` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` endpoint `GET /rad-orders/worklist` |
| Acceptance criteria | `AC-36`, `AC-40` — dan `AC-38` **tidak ada**, lihat bagian 2 |
| Test yang diminta roadmap | `UAT-13` cito di urutan pertama; `UAT-14` daftar kerja tanpa alat ditolak |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 9 — penanda cito wajib terlihat tanpa membuka rincian, dan pesanan cito wajib di urutan atas |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-13` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai.** 9 unit test baru lulus; 829 unit test repository lulus, 0 gagal |

---

## 1. Ketentuan mengikat: cito terlihat, dan cito di atas

Dua bagian, dan keduanya dipenuhi dengan cara yang berbeda.

**Cito terlihat tanpa membuka rincian.** Kolom kedua tabel adalah penanda cito, berupa lencana
bertuliskan `CITO`. Teksnya ikut, bukan hanya warnanya — pengguna dengan gangguan penglihatan
warna tetap membacanya.

**Cito di urutan atas — dipenuhi dengan TIDAK mengurutkan.** `RadOrderService.GetWorklistAsync`
sudah melakukannya:

```csharp
.OrderByDescending(x => x.IsUrgent)
.ThenBy(x => x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime)
```

Layar karena itu tidak mengurutkan apa pun, dan `sortLatestFirst` pada tabel dimatikan supaya
komponennya pun tidak. Mengurutkan ulang di klien berarti dua sumber urutan, dan yang di klien
hanya bekerja pada baris yang kebetulan sudah terbawa.

Sebagai gantinya, `assertUrutanCitoTerjaga` **memeriksa** bahwa tidak ada baris cito yang jatuh
di bawah baris biasa. Bila suatu saat ada yang menyisipkan pengurutan klien, layar menampilkan
peringatan merah alih-alih diam. Peringatan itu tidak seharusnya pernah muncul — dan justru
karena itu ia perlu terlihat bila muncul.

---

## 2. Tiga temuan

### 2.1 `AC-38` tidak ada

Roadmap `FE-RAD-07` menyebut acceptance criteria `AC-36, AC-38, AC-40`.
`testing/acceptance-test-matrix.md` memuat `AC-36`, `AC-37`, `AC-39` (dua baris), `AC-40` —
**tidak ada `AC-38`**.

Yang dimaksud hampir pasti `AC-39`: "Satu pesanan cito dan empat pesanan biasa yang lebih tua →
pesanan cito berada di urutan pertama", yang persis isi `UAT-13`. Task ini dikerjakan terhadap
`AC-39`. **Roadmap perlu diperbaiki** supaya rujukannya tidak menggantung.

### 2.2 Balasan daftar kerja tidak memuat identitas pasien

`RadWorklistItemResponse` memuat `EncounterId`, tetapi **tidak** memuat nama pasien maupun
nomor rekam medis — berbeda dari `RadOrderListResponse` yang mendapatkannya lewat
`RAD-CONF-001` bagian 8.

Akibatnya radiografer melihat pemeriksaan, alat, waktu kerja, dan status — tetapi **tidak tahu
pasien mana yang harus dipanggil**.

**Yang tidak dikerjakan, dan mengapa.** Mengambil identitas pasien per baris adalah persis pola
N+1 yang dihapus `RAD-CONF-001`. Menggabungkan balasan `GET /rad-orders` di sisi klien juga
ditolak: kedua endpoint menyaring hari kerja dengan cara berbeda, sehingga sebagian baris akan
kehilangan identitas tanpa cara apa pun bagi petugas untuk tahu mengapa.

**Yang dikerjakan.** Layar menyatakan keterbatasannya apa adanya lewat kotak keterangan:
identitas pasien wajib dipastikan dari layar verifikasi sebelum pemeriksaan dijalankan.
Acceptance criteria dan Definition of Done task ini tidak menuntut identitas pasien, sehingga
tidak ada yang tertahan — tetapi **layar ini belum cukup untuk memanggil pasien**, dan itu
perlu diketahui.

**Perbaikannya kecil dan sudah ada presedennya:** menambahkan objek `Patient` pada
`RadWorklistItemResponse`, persis seperti yang dikerjakan `RAD-CONF-001` bagian 8 untuk daftar
pesanan. Itu perubahan backend dan **perlu keputusan pemilik modul**.

### 2.3 `HoldAsync` masih menerima status terminal

Temuan `FE-RAD-06` bagian 1.2, **belum diperbaiki**. Dicatat ulang di sini karena masih terbuka.

---

## 3. `UAT-14` — daftar kerja tanpa memilih alat

Layar **tidak memanggil endpointnya sama sekali** sebelum alat dipilih, bukan memanggilnya lalu
menampilkan galat. `canLoadWorklist` menjaga itu, dan `buildWorklistParams` mengembalikan
`null` sehingga efek pemuatan tidak pernah berjalan.

Yang dilihat petugas adalah keterangan yang menyebutkan **sebabnya**: daftar kerja disusun per
alat karena penempatan petugas radiologi mengikuti ruang alat, dan tidak ada daftar gabungan
seluruh unit karena daftar seperti itu memuat ribuan baris yang tidak seorang pun minta.

---

## 4. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-worklist-rules.js` | Baru — fungsi murni; penjaga alat wajib, pemeriksa urutan cito, pembentuk parameter |
| `src/lib/hooks/health-services/radiology-management/use-rad-worklist.jsx` | Baru — controller |
| `src/lib/constants/health-services/radiology-management/rad-order-constants.jsx` | Diubah — `RAD_WORKLIST_COPY` |
| `src/components/view/health-services/radiology-management/rad-worklists/rad-worklist-view.jsx` | Baru — layar |
| `src/style/health-services/radiology-management/rad-worklists/rad-worklist.module.css` | Baru — 6 kelas, design token |
| `src/app/health-services/radiology-management/rad-worklists/…` | Baru — 2 berkas route |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — satu entri pada grup Radiologi |
| `tests/unit/rad-worklist-rules.test.mjs` | Baru — 9 test |

`store.jsx` **tidak disentuh**.

---

## 5. Definition of Done: berpindah alat tanpa berganti halaman

Pemilih alat berupa **deretan tombol**, bukan dropdown. Radiografer berpindah antar alat
berkali-kali dalam satu shift; menekan satu tombol mengganti daftar tanpa navigasi apa pun.

Setiap tombol juga membawa penanda kesiapan: alat yang belum punya aturan keselamatan berlaku
memunculkan peringatan bahwa pemeriksaannya akan ditolak gerbang. Sama seperti pada `FE-RAD-05`,
**setiap** alat memunculkannya sekarang, karena belum ada satu pun aturan `Active`.

---

## 6. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-worklist-rules.test.mjs` | **9 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **829 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 2 warning** — keduanya `react-hooks/set-state-in-effect` pada pemuatan daftar alat, jenis yang sama dengan pola repository |
| `npm run build` | **Compiled successfully in 44s.** Route terdaftar: `/health-services/radiology-management/rad-worklists` (static) |

**`MANUAL TEST: NOT FEASIBLE`.** Menjalankan layar ini menuntut pesanan radiologi pada alat
tertentu di lingkungan pengembangan beserta sesi petugas yang memegang `RadOrder : Read`; sesi
login tidak tersedia dari sini. Kedua jalur yang diminta roadmap dikunci unit test: `UAT-14`
pada `S1` — endpoint tidak dipanggil sebelum alat dipilih — dan `UAT-13` pada `S2` sampai `S5`,
termasuk kasus cito yang tenggelam di tengah daftar.

---

## 7. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Identitas pasien pada baris | Balasan backend tidak memuatnya; mengambilnya per baris adalah pola N+1 yang baru saja dihapus. Lihat bagian 2.2 |
| Aksi pada baris daftar kerja | Verifikasi pasien dan gerbang keselamatan milik `FE-RAD-08`; pengambilan citra milik `FE-RAD-09`. Daftar ini membaca saja |
| Daftar pantau keterlambatan cito | Ditunda `RAD-DEC-013`; `RAD-OPEN-009` |
| Pengurutan di sisi klien | Urutan milik server. Lihat bagian 1 |
| Base component baru | Dilarang `AGENTS.md` |

---

## 8. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **Daftar kerja tanpa identitas pasien** | Temuan bagian 2.2. Layar menyatakannya, tetapi radiografer tetap perlu layar lain untuk memastikan pasien. **Perlu keputusan pemilik modul** |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Roadmap menyebut `AC-38` yang tidak ada** | Temuan bagian 2.1. Perbaikan dokumen |
| Tidak ada aturan keselamatan `Active` | `RAD-OPEN-011` terbuka. Daftar kerja terisi, tetapi pemeriksaannya tetap ditolak gerbang |

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 6 berkas baru, 2 diubah. 9 test baru lulus; 829 test repository lulus. Tiga temuan dicatat: `AC-38` tidak ada, daftar kerja tanpa identitas pasien, dan `HoldAsync` masih terbuka. | `draft` |
