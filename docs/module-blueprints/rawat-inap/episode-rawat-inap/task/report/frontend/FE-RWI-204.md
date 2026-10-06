# Laporan Perubahan Frontend — `FE-RWI-204`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-204` |
| Judul | Daftar Kota/Kabupaten memuat jenisnya, dan hasil scan KTP membedakan Kota dari Kabupaten |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-003` butir `ISS-EPS-003-02`, `ISS-EPS-003-T1`; `PLAN-REPAIR-EPS-003` perbaikan `FIX-EPS-003-02`, `FIX-EPS-003-03` |
| Contract version | `0.10.0` — tidak disentuh. Endpoint `GET /api/v1/administrator/master-data/regions/cities/options` dipakai apa adanya |
| Dependency | Tidak ada. Backend sudah mengirim jenis kota pada `RegionOptionResponse.AdditionalInfo` (`RegionController.cs:1626`) |
| Klasifikasi | `MEDIUM` — mengubah hasil isi otomatis hasil scan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | `PLAN-REPAIR-EPS-003` yang diperintahkan untuk diimplementasikan pemilik 6 Oktober 2026; skema tampilan 3.3 revision `0.6` baris Keadaan "Wilayah hasil scan KTP tidak dapat dicocokkan" |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b010ffb9722f236160477c95c931c22467550200` (`HamzahV2`), perubahan belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

**Label kembar.** Master `MstCity` memuat dua wilayah berbeda bernama "Bekasi" pada Jawa Barat —
`32.16` Kabupaten dan `32.75` Kota. Backend mengirim jenisnya pada `additionalInfo`, tetapi frontend
membaca `cityType`/`cityTypeName` yang tidak pernah ada, sehingga keduanya berlabel "Bekasi".

**Pencocokan scan yang menebak.** `normalizeLookupText` membuang kata KOTA/KABUPATEN sebelum
mencocokkan teks KTP, sehingga "KOTA BEKASI" dan "KABUPATEN BEKASI" bernilai sama, dan yang menang
adalah yang kebetulan datang lebih dulu dari backend.

## 2. Proses bisnis

1. Petugas memilih Provinsi Jawa Barat; daftar Kota/Kabupaten kini memuat "Kabupaten Bekasi" dan
   "Kota Bekasi" sebagai dua pilihan yang dapat dibedakan.
2. Petugas yang memindai KTP berdomisili "KOTA BEKASI" mendapat isian "Kota Bekasi" beserta daftar
   kecamatan Kota Bekasi.
3. Bila KTP hanya terbaca "BEKASI", sistem **tidak menebak**: isian dibiarkan kosong, dan peringatan di
   bawah panel scan menyebut *"Wilayah berikut belum dapat dicocokkan otomatis dan perlu dipilih manual:
   Kota/Kabupaten BEKASI."*

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/registration-management/emergency-region.service.js` | `getCityTypeKey` mengenali KOTA/KABUPATEN/KAB. dengan batas kata (tidak cocok dengan "Kotawaringin"). `normalizeRegionOption` membaca `additionalInfo` untuk tipe kota, mengembalikan `cityType`, dan tidak menggandakan jenis bila nama sudah memuatnya. `scoreRegionOption` menambah 300 bila jenis cocok dan mengurangi 250 bila tidak — bobot yang sama dengan kiosk. `findBestRegionOption` mengembalikan `null` bila dua kota teratas seri |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | State `scanRegionNotice` diisi dari `region.missing` sesudah scan; dikosongkan setiap scan baru |
| `src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` | `InformationAlert` peringatan di bawah panel scan menampilkan `scanRegionNotice` |

### 3.2 Radius dampak

| Pemakai | Dampak |
| --- | --- |
| Pendaftaran IGD (`use-emergency-region-option.js`, `use-emergency-registration.js`) | Ikut mendapat label berjenis dan pencocokan yang tidak menebak. IGD sudah meneruskan `missing` sebagai `missingRegionFields` sendiri |
| Kiosk pendaftaran | Tidak berubah — memakai helper kiosk sendiri yang sejak awal membaca `additionalInfo` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — field `additionalInfo` sudah ada pada respons |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Daftar Kota/Kabupaten | `REUSE` | `EmergencySelectField` → `FilterSelect`; hanya label opsinya yang berubah |
| Peringatan wilayah hasil scan | `REUSE` | `InformationAlert` varian `warning` |

`UI GATE: PASS`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-registration-issue-003.test.mjs` | 9 dari 9 lulus; lima di antaranya untuk task ini: label berjenis, KOTA BEKASI → Kota, KABUPATEN/KAB. BEKASI → Kabupaten, BEKASI tanpa jenis tidak ditebak, kota tanpa nama kembar tetap terisi | `PASS` |
| `npx eslint` berkas yang diubah | 0 error, 0 warning baru | `PASS` |
| `npx eslint src --quiet` | Exit `0` | `PASS` |
| `npm run build` | Exit `0`; 474 halaman; standalone siap | `PASS` |
| Uji peramban pada Jawa Barat dan scan KTP nyata | Belum dijalankan | `NOT RUN` |

Saat menyusun test, satu cacat sungguhan tertangkap: ekspresi `\b` pada `getCityTypeKey` sempat
tertulis sebagai karakter *backspace* oleh skrip penyunting, sehingga jenis kota tidak pernah terbaca.
Diperbaiki, dan seluruh `src/` dipindai ulang — nol karakter serupa.

`MANUAL TEST: NOT RUN` — dikecualikan atas keputusan pemilik 1 September 2026 dan 10 September 2026.

`AUTOMATED TEST: node --test tests/unit/inpatient-admission-registration-issue-003.test.mjs — PASS (9/9)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Jawa Barat memuat "Kabupaten Bekasi" dan "Kota Bekasi"; tidak ada dua label identik | Terpenuhi | `normalizeRegionOption`; unit test label |
| Pencarian "bekasi" memunculkan keduanya lengkap dengan jenis | Terpenuhi | Label opsi memuat jenis; pencarian `FilterSelect` bekerja atas label |
| Setelah dipilih, isian menampilkan "Kota Bekasi" | Terpenuhi | Label opsi terpilih sama dengan label daftar |
| Kota tanpa `CityType` tampil dengan nama saja | Terpenuhi | Unit test opsi "Contoh" |
| Pendaftaran IGD ikut menampilkan jenis kota | Terpenuhi | Service yang sama |
| KTP "KOTA BEKASI" → Kota Bekasi | Terpenuhi | Unit test |
| KTP "KABUPATEN BEKASI"/"KAB. BEKASI" → Kabupaten Bekasi | Terpenuhi | Unit test |
| KTP hanya "BEKASI" → kosong dan petugas diberi tahu | Terpenuhi | Unit test (`missing`) + `scanRegionNotice` |
| Kota tanpa nama kembar tetap terisi otomatis | Terpenuhi | Unit test KOTA DEPOK, Kotawaringin Barat |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar cakupan | Logika pencocokan wilayah kini ada di dua tempat dengan bobot yang sama — kiosk dan service IGD. Penyatuan ke satu util bersama tetap disarankan (`ISSUE-EPS-003` bagian 8 pola 3), tidak dikerjakan agar kiosk tidak tersentuh |
| Risiko tersisa | Isi otomatis sedikit lebih jarang untuk KTP yang tidak menyebut jenis kota — disengaja demi kebenaran data. Verifikasi peramban belum dijalankan |
| Perubahan sampingan | `NONE` |
