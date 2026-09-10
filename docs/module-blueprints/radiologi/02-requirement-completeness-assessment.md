# Radiologi — Requirement Completeness Assessment

| Field | Value |
|---|---|
| Assessment ID | `RAD-RCG-001` |
| Revision | `2` |
| Status | `draft` |
| Blueprint ID | `RAD-BP-001` |
| **Kesiapan keseluruhan** | **`PARTIALLY_READY`** — **12** dari 15 slice boleh maju ke arsitektur domain |
| Perubahan revision 2 | `S12` naik menjadi siap setelah `DEC-RAD-003` ditutup `RAD-DEC-012` dan `RAD-DEC-013` |
| Backend SHA | `64da911` |
| Frontend SHA | `f66ed1885` |
| Masukan | `00-interview-decisions.md` revision 7; `01-existing-capability-map.md` revision 1 |
| Tanggal penilaian | 2026-09-09 |
| Task mode | `AUDIT MODE` untuk source, `MODULE BLUEPRINT MODE` untuk penulisan dokumen |

> **Apa yang dikerjakan dokumen ini.** Dokumen ini memeriksa apakah kebutuhan bisnis modul
> Radiologi sudah cukup jelas untuk dirancang arsitektur domainnya. Ia **tidak** merancang
> tabel, tidak membuat kontrak API, dan tidak menjawab pertanyaan bisnis atas nama rumah sakit.
>
> Hasilnya berupa satu vonis per bagian: boleh maju, atau harus berhenti dan menunggu keputusan.

---

## 1. Scope Penilaian

**Modul:** Radiologi (`radiologi`).

Modul dipecah menjadi 15 slice — bagian terkecil yang masih bermakna dan bisa dipikirkan
sendiri. Pemecahan ini penting supaya satu bagian yang macet tidak menahan seluruh modul.

> **Contoh mengapa pemecahan itu perlu.** Pencatatan hasil bacaan radiolog sudah jelas
> aturannya dan boleh dirancang sekarang. Sedangkan daftar kerja petugas belum pernah dibahas
> sama sekali. Kalau modul dinilai sebagai satu blok, hasil bacaan ikut tertahan tanpa alasan.

| Slice | Nama | Keadaan di source `64da911` |
|---|---|---|
| `S1` | Pesanan radiologi | Sudah ada |
| `S2` | Study dan pengambilan citra | Sudah ada |
| `S3` | Penilaian gerbang keselamatan | Sudah ada |
| `S4` | Pengelolaan aturan keselamatan | Belum bisa dikelola |
| `S5` | Pelewatan gerbang keselamatan darurat | Belum ada |
| `S6` | Penilaian mutu citra, pengulangan, penghentian | Sudah ada |
| `S7` | Pencatatan pemakaian bahan | Sudah ada |
| `S8` | Fakta kelayakan tagih ke Billing | Sudah ada |
| `S9` | Hasil bacaan radiolog | Belum ada |
| `S10` | Koreksi hasil berversi | Belum ada |
| `S11` | Temuan kritis | Belum ada |
| `S12` | Daftar kerja petugas radiologi | Belum ada |
| `S13` | Data induk alat pencitraan | Belum bisa dikelola |
| `S14` | Penyajian hasil ke rekam medis | Belum ada |
| `S15` | Tampilan frontend Radiologi | Belum ada |

**Di luar penilaian ini:** kedokteran nuklir, radioterapi, PACS/DICOM, stok bahan,
perhitungan tarif, dan dosis radiasi petugas. Semuanya sudah dikeluarkan dari scope lewat
`RAD-DEC-001` dan `RAD-DEC-002`.

---

## 2. Bukti yang Dipakai

Urutan wewenang mengikuti kontrak gerbang: requirement rumah sakit terkini di atas, bukti
implementasi di bawahnya.

| Sumber | Jenis | Wewenang | Dipakai untuk |
|---|---|---|---|
| `00-interview-decisions.md` rev 7 | Keputusan pemilik modul, 11 keputusan terkunci | Tingkat 1 — requirement eksplisit dari pemilik | Aturan bisnis, kewenangan, lifecycle |
| `RJ-BIL-GATE-DEC-004` | Keputusan terkunci pada blueprint `rawat-jalan` | Tingkat 1, diwariskan | Lifecycle pesanan, study, laporan; invariant keselamatan dan tagih |
| `RJ-BIL-DEC-014` | Keputusan penunjukan pemilik modul | Tingkat 1, diwariskan | Sifat data induk keselamatan, perilaku fail-closed |
| `IGD-DEC-099` | Keputusan modul IGD, status `draft` | Tingkat 1, diwariskan — **sudah usang** | Titik sentuh IGD |
| `01-existing-capability-map.md` rev 1 | Audit source | Tingkat 6 — bukti implementasi V2 | Apa yang saat ini ada |
| Source `64da911` dan `f66ed1885` | Kode dan test | Tingkat 6 | Perilaku terverifikasi |

**Yang tidak tersedia dan berpengaruh pada penilaian:**

- Tidak ada SOP radiologi rumah sakit yang disahkan.
- Tidak ada notulen rapat klinis.
- Tidak ada bukti ClickUp yang disetujui untuk modul ini.
- Penanggung jawab tata kelola klinis belum ditunjuk.

Ketiadaan itu bukan alasan menganggap requirement tidak ada. Ia berarti sebagian pertanyaan
memang belum pernah diajukan kepada pihak yang berwenang menjawabnya.

---

## 3. Temuan Kelengkapan per Dimensi

Ringkasan 18 dimensi terhadap seluruh slice. Kolom terakhir menunjuk slice yang bermasalah.

| ID | Dimensi | Keadaan | Slice bermasalah |
|---|---|---|---|
| 01 | Tujuan | Jelas untuk 14 slice | `S12` |
| 02 | Aktor | Jelas untuk 13 slice | `S12`, sebagian `S5` |
| 03 | Pemicu / prasyarat | Jelas untuk 14 slice | `S12` |
| 04 | Alur utama | Jelas untuk 14 slice | `S12` |
| 05 | Alur alternatif / exception | Jelas — pembatalan, penolakan, penghentian, pengulangan sudah dikunci | — |
| 06 | Data minimum | Jelas untuk 13 slice | `S11`, `S12` |
| 07 | Aturan bisnis / validation | Jelas untuk 13 slice | `S11`, `S12` |
| 08 | Status / perubahan status | Jelas — 21 status sudah terkunci di kode dan keputusan | — |
| 09 | Peran / authorization | **Aturan jelas, pemetaan peran belum** | Seluruh slice berpagar wewenang |
| 10 | Dependency antarmodul | Jelas — Registration, MasterData, Billing, InPatient, ClinicalManagement | — |
| 11 | Integrasi eksternal | Jelas — tidak ada; RIS/PACS sengaja tidak diaktifkan | — |
| 12 | Hasil akhir | Jelas untuk 14 slice | `S12` |
| 13 | Pembatalan / koreksi | Jelas — koreksi berversi dikunci `RJ-BIL-GATE-DEC-004` | — |
| 14 | Audit / histori | Jelas — `RadTransitionHistory` sudah berjalan | — |
| 15 | Notifikasi | Jelas untuk temuan kritis lewat `RAD-DEC-004` dan `RAD-DEC-010` | — |
| 16 | Dampak billing | Jelas — satu fakta per study, hanya saat citra layak | — |
| 17 | Dampak keselamatan klinis | **Sebagian belum disahkan pihak berwenang** | `S5`, `S11` |
| 18 | Pelaporan / traceability | Jelas — jejak `Patient → Encounter → Order → Study` sudah berjalan | — |

### Catatan pada dimensi 09 — Peran dan authorization

Aturan **siapa boleh apa** sudah jelas dan terkunci. Yang belum ada adalah terjemahannya ke
peran yang benar-benar terpasang di Quilvian.

Empat sebutan peran dipakai keputusan, belum satu pun dipetakan:

| Sebutan dalam keputusan | Dipakai oleh | Peran Quilvian yang setara |
|---|---|---|
| Dokter radiolog | `RAD-DEC-003` — mengesahkan hasil bacaan | Belum dipetakan |
| Penanggung jawab klinis | `RAD-DEC-005` — mengesahkan aturan keselamatan | Belum dipetakan |
| DPJP | `RAD-DEC-008` — mengesahkan pelewatan darurat | Belum dipetakan |
| Dokter jaga senior | `RAD-DEC-008` — mengesahkan pelewatan darurat | Belum dipetakan |

**Ini tidak memblokir arsitektur domain.** Arsitektur dapat menyatakan "pengesah wajib
berperan dokter radiolog" tanpa mengetahui nomor peran di database. Yang tertahan adalah
implementasinya.

### Catatan pada dimensi 17 — Keselamatan klinis

Dua slice menyentuh keselamatan pasien dan belum disahkan pihak yang berwenang:

- `S5` pelewatan gerbang keselamatan — diputuskan pemilik modul, **tanpa** tanda tangan tata
  kelola klinis.
- `S11` temuan kritis — mekanismenya jelas, tetapi **daftar apa yang dihitung kritis belum
  ada**.

Keduanya dinilai `BLOCKING` pada bagian 5.

---

## 4. Klasifikasi Bukti per Slice

### Slice yang bukti requirement-nya `CONFIRMED`

| Slice | Bukti | Keterangan |
|---|---|---|
| `S1` Pesanan radiologi | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-011`, `RAD-DEC-009`, source terverifikasi | Lifecycle, kewenangan, dan titik sentuh IGD lengkap |
| `S2` Study | `RJ-BIL-GATE-DEC-004`, source terverifikasi | Enam status jalur normal dan lima jalur pengecualian terkunci |
| `S3` Penilaian gerbang keselamatan | `RJ-BIL-DEC-014`, `RadSafetyGateEvaluator` terverifikasi | Fail-closed terbukti dari kode dan test |
| `S4` Pengelolaan aturan keselamatan | `RAD-DEC-005` | Alur pengesahan dan versioning lengkap |
| `S6` Mutu, pengulangan, penghentian | `RJ-BIL-GATE-DEC-004`, source terverifikasi | Sebab pengulangan dan penghentian sudah berdaftar |
| `S7` Pemakaian bahan | `RJ-BIL-GATE-DEC-004`, `RJ-BIL-GATE-DEC-005` | Dicatat sebagai jumlah, bukan rupiah |
| `S8` Fakta tagih | Kontrak `BIL-INTEGRATION-0.4` terverifikasi | Satu fakta per study, hanya saat citra layak |
| `S9` Hasil bacaan | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003`, `RAD-DEC-006` | Lifecycle, kewenangan pengesahan, dan jalur ke rekam medis lengkap |
| `S10` Koreksi berversi | `RJ-BIL-GATE-DEC-004` | Tiga status amandemen terkunci, versi lama tidak boleh ditimpa |
| `S13` Data induk alat | `RAD-DEC-001` butir 14, pola master data project | Kebutuhan kelola standar |
| `S14` Penyajian ke rekam medis | `RAD-DEC-006` | Baca langsung, tanpa salinan |

### Slice yang bukti requirement-nya `PROPOSED`, `MISSING`, atau `CONFLICT`

| Slice | Butir | Status bukti | Penjelasan |
|---|---|---|---|
| `S5` Pelewatan darurat | Seluruh mekanisme | `PROPOSED` | Diputuskan pemilik modul lewat `RAD-DEC-008`, tetapi keselamatan klinis bukan wewenangnya sendiri. Tanda tangan tata kelola klinis belum ada, dan `RJ-BIL-GATE-DEC-004` mensyaratkannya |
| `S11` Temuan kritis | Daftar apa yang dihitung kritis | `MISSING` | Mekanismenya jelas, isinya belum ada. Tanpa daftar, penandaan bergantung selera masing-masing radiolog |
| `S12` Daftar kerja petugas | Seluruh dimensi | `MISSING` | **Belum pernah dibahas sama sekali.** Tidak ada keputusan yang menyebutnya selain mencantumkannya di daftar scope |
| Lintas slice | Pemetaan empat peran | `MISSING` | Aturannya jelas, terjemahan ke peran Quilvian belum ada |
| `S1` Titik sentuh IGD | Modul IGD masih menyatakan Radiologi belum ada | `CONFLICT` | Jalan keluar sudah ditetapkan `RAD-DEC-009`, belum dijalankan |
| Lintas slice | Registry `Rad` masih `PLANNED` | `CONFLICT` | Jalan keluar sudah ditetapkan `RAD-DEC-007`, belum dijalankan |

---

## 5. Klasifikasi Dampak Gap

| Butir | Dampak | Menahan apa | Alasan klasifikasi |
|---|---|---|---|
| `S5` pelewatan darurat belum disahkan klinis | **`BLOCKING`** | Arsitektur domain `S5` | Kalau tata kelola klinis kelak menolak konsep pelewatan, seluruh struktur penyimpanannya berubah. Merancangnya sekarang berisiko dibongkar ulang |
| `S11` daftar temuan kritis belum ada | **`BLOCKING`** | Arsitektur domain `S11` | Bukan sekadar isi data. Kalau ada daftar resmi, dibutuhkan tabel induk temuan kritis; kalau tidak ada, cukup satu penanda pada hasil. Dua jawaban itu menghasilkan struktur berbeda |
| `S12` daftar kerja belum pernah dibahas | **`BLOCKING`** | Arsitektur domain `S12` | Tidak ada tujuan, pelaku, pengelompokan, maupun penyaringan yang diketahui. Tidak ada yang bisa dirancang |
| Pemetaan empat peran | `NON_BLOCKING_STANDARD` | Implementasi, bukan desain | Arsitektur dapat menyebut peran secara bisnis; pemetaan ke peran Quilvian adalah konfigurasi |
| Registry `Rad` masih `PLANNED` | `NON_BLOCKING_STANDARD` | Implementasi, bukan desain | Merancang tabel tidak sama dengan membuatnya. Yang tertahan `QBE-MOD-002` adalah pembuatannya |
| Konflik IGD | `NON_BLOCKING_STANDARD` | Perbaikan modul IGD | Endpoint Radiologi tidak berubah karenanya |
| `S4` sumber nilai awal aturan keselamatan | `CONFIGURABLE_DEFAULT` | Tidak menahan apa pun | Isi awal memang wajar berbeda antar rumah sakit. Strukturnya sudah ditetapkan `RAD-DEC-005` |
| Uji kontrak hak akses radiologi | `NON_BLOCKING_STANDARD` | Tidak menahan desain | Praktik baik yang sudah dipakai modul sebanding |

### Mengapa registry dan pemetaan peran tidak dinilai memblokir desain

Ini pembedaan yang menentukan hasil penilaian, jadi perlu dijelaskan.

Arsitektur domain menghasilkan **rancangan**: bounded context, aggregate, lifecycle, dan
kepemilikan konsep. Ia tidak membuat tabel, tidak membuat migration, dan tidak menulis kode.

Registry `QBE-MOD-002` menahan **pembuatan** entity, bukan perancangannya. Begitu pula
pemetaan peran: arsitektur cukup menyatakan "pengesah wajib dokter radiolog", dan penerjemahan
ke peran Quilvian terjadi saat implementasi.

Menahan arsitektur domain karena keduanya akan menunda pekerjaan yang sebenarnya bisa jalan,
tanpa mengurangi risiko apa pun.

---

## 6. Decision Log

Enam keputusan yang perlu diselesaikan pemilik berwenang. **Tidak satu pun dijawab dokumen
ini.** Penutupannya lewat `/grill-me`.

### `DEC-RAD-001` — Pengesahan klinis atas pelewatan gerbang keselamatan

| Field | Isi |
|---|---|
| **Pertanyaan** | Apakah tata kelola klinis menyetujui adanya jalur pelewatan gerbang keselamatan darurat sebagaimana dirumuskan `RAD-DEC-008`, dan apakah syarat pengesahnya sudah tepat? |
| **Slice terdampak** | `S5` |
| **Bukti saat ini** | `RAD-DEC-008` disetujui pemilik modul 2026-09-09 tanpa countersignature klinis. `RJ-BIL-GATE-DEC-004` mensyaratkan pengesahan terpisah |
| **Usulan baseline** | Pertahankan rumusan `RAD-DEC-008`: pelewatan hanya oleh DPJP atau dokter jaga senior, wajib alasan tertulis, ditandai permanen, masuk tinjauan berkala |
| **Dampak** | Keselamatan klinis, lifecycle study, authorization, audit |
| **Pemilik** | Penanggung jawab tata kelola klinis — **belum ditunjuk** |
| **Status** | `OPEN` |
| **Dampak domain** | `S5` berhenti. Slice lain tidak bergantung padanya |

### `DEC-RAD-002` — Daftar temuan kritis radiologi

| Field | Isi |
|---|---|
| **Pertanyaan** | Apa saja yang dihitung sebagai temuan kritis, dan apakah daftarnya ditetapkan resmi sebagai data induk atau diserahkan pada penilaian radiolog? |
| **Slice terdampak** | `S11` |
| **Bukti saat ini** | `RAD-DEC-004` mengunci mekanismenya. Isinya belum pernah ditetapkan |
| **Usulan baseline** | Tetapkan sebagai data induk yang dapat diubah, mengikuti pola aturan keselamatan pada `RAD-DEC-005`. Contoh butir yang lazim: perdarahan intrakranial, pneumotoraks luas, udara bebas intraabdomen, diseksi aorta |
| **Dampak** | Struktur domain, keselamatan klinis, notifikasi |
| **Pemilik** | Penanggung jawab tata kelola klinis — **belum ditunjuk** |
| **Status** | `OPEN` |
| **Dampak domain** | `S11` berhenti. `S9` dan `S10` tidak bergantung padanya |

> **Mengapa ini mengubah struktur, bukan sekadar isi.** Kalau ada daftar resmi, dibutuhkan
> tabel induk temuan kritis, dan hasil bacaan menunjuk ke butir daftar itu. Kalau tidak ada,
> cukup satu kolom penanda pada hasil bacaan. Tabel induk dan kolom penanda adalah dua
> rancangan yang berbeda, dan mengubahnya belakangan berarti migration.

### `DEC-RAD-003` — Kebutuhan daftar kerja petugas radiologi — **DITUTUP**

| Field | Isi |
|---|---|
| **Pertanyaan** | Daftar kerja petugas radiologi dikelompokkan menurut apa? Apakah ada penandaan mendesak? |
| **Slice terdampak** | `S12` |
| **Bukti saat penilaian revision 1** | Tidak ada. Daftar kerja hanya disebut sebagai kemampuan dalam scope `RAD-DEC-001`, tanpa satu pun rincian |
| **Jawaban** | Dikelompokkan **per alat pencitraan** (`RAD-DEC-012`). Ada **penanda cito** pada pesanan yang diisi dokter pengirim (`RAD-DEC-013`) |
| **Status** | **`CLOSED` 2026-09-09** lewat Amendment pass |
| **Dampak domain** | `S12` naik menjadi `READY_FOR_DOMAIN_DESIGN` |
| **Catatan penting** | Bentuk yang dipilih **tidak membutuhkan tabel baru**. Daftar kerja berupa penyaringan atas data yang sudah ada, sehingga `S12` **tidak** ikut tertahan `RAD-CONFLICT-001` |

Sisa yang belum diputuskan dicatat terpisah sebagai `RAD-OPEN-009`: apakah pesanan cito perlu
dipantau keterlambatannya, dan berapa batas waktunya. Itu **tidak memblokir** apa pun — hanya
perluasan `POST-MVP`.

### `DEC-RAD-004` — Pemetaan empat peran ke peran Quilvian

| Field | Isi |
|---|---|
| **Pertanyaan** | Peran mana di Quilvian yang setara dengan dokter radiolog, penanggung jawab klinis, DPJP, dan dokter jaga senior? |
| **Slice terdampak** | `S4`, `S5`, `S9`, `S11` |
| **Bukti saat ini** | Empat sebutan dipakai `RAD-DEC-003`, `RAD-DEC-005`, dan `RAD-DEC-008`; belum satu pun dipetakan |
| **Usulan baseline** | Petakan saat implementasi, mengikuti pola `[AccessPermission(...)]` yang sudah dipakai 26 endpoint radiologi |
| **Dampak** | Authorization pada saat implementasi |
| **Pemilik** | Pemilik modul + Administrator |
| **Status** | `OPEN` |
| **Dampak domain** | **Tidak menahan** arsitektur domain. Menahan implementasi |

### `DEC-RAD-005` — Sumber nilai awal aturan keselamatan

| Field | Isi |
|---|---|
| **Pertanyaan** | Isi awal aturan keselamatan disiapkan tim sebagai data bawaan, atau seluruhnya diketik admin rumah sakit sendiri? |
| **Slice terdampak** | `S4` |
| **Bukti saat ini** | `RJ-BIL-DEC-014` menyatakan baseline standardisasi Indonesia bersifat **tidak otoritatif** dan wajib diverifikasi terhadap SOP rumah sakit yang sebenarnya |
| **Usulan baseline** | Sediakan data bawaan sebagai usulan berstatus `Draf`, sehingga tetap harus disahkan penanggung jawab klinis sebelum berlaku |
| **Dampak** | Kesiapan pakai modul, bukan struktur |
| **Pemilik** | Penanggung jawab tata kelola klinis |
| **Status** | `OPEN` |
| **Dampak domain** | **Tidak menahan.** Strukturnya sudah ditetapkan `RAD-DEC-005` |

### `DEC-RAD-006` — Kewajiban uji kontrak hak akses

| Field | Isi |
|---|---|
| **Pertanyaan** | Apakah uji kontrak hak akses radiologi wajib ada sebelum Rilis 1, mengikuti pola Laboratorium dan Bank Darah? |
| **Slice terdampak** | Seluruh slice |
| **Bukti saat ini** | Modul Laboratorium punya `LaboratoryAuthorityTests`; Bank Darah punya `BloodBankRoleAccessContractTests`; Radiologi tidak punya |
| **Usulan baseline** | Wajibkan, mengikuti dua modul sebanding |
| **Dampak** | Mutu, bukan makna domain |
| **Pemilik** | Pemilik modul |
| **Status** | `OPEN` |
| **Dampak domain** | **Tidak menahan** |

---

## 7. Kesiapan per Slice

| Slice | Nama | Kesiapan | Blocker |
|---|---|---|---|
| `S1` | Pesanan radiologi | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S2` | Study dan pengambilan citra | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S3` | Penilaian gerbang keselamatan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S4` | Pengelolaan aturan keselamatan | **`READY_FOR_DOMAIN_DESIGN`** | — (isi awal `DEC-RAD-005` tidak menahan struktur) |
| `S5` | Pelewatan gerbang darurat | **`BUSINESS_DECISION_REQUIRED`** | `DEC-RAD-001` |
| `S6` | Mutu, pengulangan, penghentian | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S7` | Pencatatan pemakaian bahan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S8` | Fakta kelayakan tagih | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S9` | Hasil bacaan radiolog | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S10` | Koreksi hasil berversi | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S11` | Temuan kritis | **`BUSINESS_DECISION_REQUIRED`** | `DEC-RAD-002` |
| `S12` | Daftar kerja petugas | **`READY_FOR_DOMAIN_DESIGN`** | — (ditutup `RAD-DEC-012` dan `RAD-DEC-013` pada 2026-09-09) |
| `S13` | Data induk alat pencitraan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S14` | Penyajian hasil ke rekam medis | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S15` | Tampilan frontend | **`PARTIALLY_READY`** | Mengikuti slice backend yang dilayaninya |

**Kesiapan keseluruhan: `PARTIALLY_READY`.**

### Dependency antar slice

| Slice | Bergantung pada | Keterangan |
|---|---|---|
| `S10` koreksi berversi | `S9` hasil bacaan | Tidak ada yang dikoreksi bila hasil bacaan belum ada |
| `S11` temuan kritis | `S9` hasil bacaan | Penanda kritis melekat pada hasil bacaan |
| `S14` penyajian ke rekam medis | `S9` hasil bacaan | — |
| `S5` pelewatan darurat | `S3` dan `S4` | Pelewatan hanya bermakna bila gerbangnya ada dan aturannya dapat dikelola |
| `S15` frontend | Seluruh slice backend | Layar mengikuti kemampuan yang dilayaninya |

> **Catatan penting soal `S11`.** Temuan kritis bergantung pada hasil bacaan, tetapi hasil
> bacaan **tidak** bergantung pada temuan kritis. Karena itu `S9` boleh dirancang sekarang
> walaupun `S11` tertahan. Yang perlu dijaga saat merancang `S9`: sediakan ruang bagi penanda
> kritis tanpa menetapkan bentuknya, supaya penambahannya nanti tidak membongkar struktur.

---

## 8. Apa yang Boleh Berjalan

Dua belas slice berikut boleh diteruskan ke `/hospital-domain-architect`:

`S1`, `S2`, `S3`, `S4`, `S6`, `S7`, `S8`, `S9`, `S10`, `S12`, `S13`, `S14`.

`S12` bergabung pada revision 2 setelah `DEC-RAD-003` ditutup.

Yang penting: **inti Rilis 1 termasuk di dalamnya.** Hasil bacaan radiolog (`S9`) dan koreksi
berversi (`S10`) — dua kemampuan yang menjadi alasan utama Rilis 1 ada — sudah cukup jelas
requirement-nya untuk dirancang.

Alasannya karena `RJ-BIL-GATE-DEC-004` sudah mengunci lifecycle-nya sejak 19 Agustus 2026,
`RAD-DEC-003` mengunci siapa yang boleh mengesahkan, dan `RAD-DEC-006` mengunci jalur ke rekam
medis. Tiga hal itu cukup untuk merancang aggregate dan lifecycle-nya.

---

## 9. Apa yang Harus Berhenti

| Slice | Berhenti sampai | Yang dibutuhkan |
|---|---|---|
| `S5` pelewatan darurat | `DEC-RAD-001` ditutup | Tanda tangan tata kelola klinis |
| `S11` temuan kritis | `DEC-RAD-002` ditutup | Daftar temuan kritis dari tata kelola klinis |

**Dua slice ini tidak boleh dikirim ke arsitektur domain.** Merancangnya sekarang berarti
agent yang menentukan sendiri bagaimana keselamatan pasien dimodelkan.

Keduanya menunggu pemilik yang sama: **penanggung jawab tata kelola klinis yang belum
ditunjuk**.

---

## 10. Keputusan Pemilik yang Dibutuhkan

Diurutkan menurut seberapa besar yang tertahan.

| Urutan | Decision ID | Pemilik | Menahan |
|---:|---|---|---|
| 1 | `DEC-RAD-001` | Tata kelola klinis — **belum ditunjuk** | `S5` |
| 2 | `DEC-RAD-002` | Tata kelola klinis — **belum ditunjuk** | `S11` |
| ~~3~~ | ~~`DEC-RAD-003`~~ | **DITUTUP 2026-09-09** oleh `RAD-DEC-012` dan `RAD-DEC-013` | — |
| 4 | `DEC-RAD-004` | Pemilik modul + Administrator | Implementasi 4 slice |
| 5 | `DEC-RAD-005` | Tata kelola klinis | Kesiapan pakai `S4` |
| 6 | `DEC-RAD-006` | Pemilik modul | Mutu, tidak menahan |

**Akar kedua blocker yang tersisa sama: penanggung jawab tata kelola klinis belum ditunjuk.**
Selama peran itu kosong, `S5` dan `S11` tidak dapat bergerak, dan `RAD-DEC-008` tidak boleh
dipakai di lingkungan production.

`DEC-RAD-003` sudah ditutup pada Amendment pass 2026-09-09. Ia memang berbeda sifatnya — tidak
butuh tata kelola klinis, hanya butuh satu sesi tanya jawab dengan pemilik modul.

---

## 11. Handoff Berikutnya

| Kondisi | Skill | Yang dikirim |
|---|---|---|
| 12 slice siap | `/hospital-domain-architect` | `S1`, `S2`, `S3`, `S4`, `S6`, `S7`, `S8`, `S9`, `S10`, `S12`, `S13`, `S14` — dinyatakan **independen** dari dua slice yang tertahan |
| 2 slice tertahan | `/grill-me` Amendment pass | `DEC-RAD-001`, `DEC-RAD-002` |
| Konflik registry dan IGD | Di luar skill mana pun | Eksekusi `RAD-DEC-007` dan `RAD-DEC-009` oleh pemiliknya |

### Yang wajib dibawa ke `/hospital-domain-architect`

| Butir | Nilai |
|---|---|
| Identitas modul | `radiologi`, blueprint `RAD-BP-001` |
| Assessment ID | `RAD-RCG-001-r2` |
| Snapshot bukti | BE `64da911`, FE `f66ed1885` |
| Slice yang dikirim | `S1`, `S2`, `S3`, `S4`, `S6`, `S7`, `S8`, `S9`, `S10`, `S12`, `S13`, `S14` |
| Slice yang **tidak** dikirim | `S5`, `S11` |
| Decision ID belum selesai | `DEC-RAD-001`, `DEC-RAD-002`, `DEC-RAD-004`, `DEC-RAD-005`, `DEC-RAD-006` |
| Keputusan yang mengikat | `RJ-BIL-GATE-DEC-004`, `RJ-BIL-DEC-014`, `RAD-DEC-001` sampai `RAD-DEC-013` |
| Kesiapan | `PARTIALLY_READY` |
| Keluaran yang diharapkan | `03-domain-architecture.md` |

**Catatan yang harus disampaikan ke arsitektur domain:** saat merancang `S9`, sediakan ruang
bagi penanda temuan kritis tanpa menetapkan bentuknya. `S11` akan menyusul setelah
`DEC-RAD-002` ditutup, dan penambahannya sebaiknya tidak membongkar struktur hasil bacaan.

---

## 12. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Penilaian pertama. 15 slice dinilai, 11 siap, 3 tertahan, 1 mengikuti. Enam Decision ID diterbitkan. | `draft` |
| 2 | 2026-09-09 | `DEC-RAD-003` ditutup lewat Amendment pass. `S12` naik menjadi `READY_FOR_DOMAIN_DESIGN` dan **tidak** tertahan blocker registry karena bentuknya tidak membutuhkan tabel baru. Slice siap menjadi 12, tertahan menjadi 2. | `draft` |
