# Aturan Pra-Scan Wajib Sebelum Wawancara Modul

| Field | Nilai |
| --- | --- |
| Status | Canonical rule |
| Berlaku untuk | `/grill-me`, `/trace-existing-capabilities`, `/design-business-module`, `/build-module-backend` |
| Lokasi canonical | `NewQuilvianSystemBackend/.claude/rules/rule-prascan/aturan-prascan-modul.md` |
| Wajib dibaca sebelum | Memulai wawancara modul baru atau membuat entity/endpoint baru |
| Format keluaran | [format-registry-sistem.md](format-registry-sistem.md) |

Aturan ini menutup satu celah nyata: wawancara bisnis selama ini dimulai tanpa seorang pun
tahu persis apa yang **sudah** ada di dalam sistem. Akibatnya modul baru mengusulkan tabel
yang sebenarnya sudah tersedia, memakai nama yang sudah dipakai modul lain, atau mengambil
alih data yang pemiliknya modul lain.

Backend Quilvian saat ini memuat **445 `DbSet`** dan **246 controller**. Tidak ada manusia yang
bisa mengingat isi sistem sebesar itu. Karena itu ingatan diganti dokumen.

---

## 1. Aturan inti

> **Tidak boleh ada wawancara modul sebelum registry sistem tersedia dan segar.**

Tiga kewajiban yang mengikat:

1. **Wajib scan lebih dulu.** `/grill-me` berhenti sebelum pertanyaan pertama bila registry
   sistem belum ada, atau statusnya bukan `SEGAR`.
2. **Wajib menampilkan Kartu Konteks.** Sebelum pertanyaan pertama, agent menampilkan
   ringkasan satu layar tentang apa yang sudah terdaftar dan bersinggungan dengan modul yang
   akan dibahas.
3. **Wajib memisahkan fakta dari usulan.** Registry hanya berisi keadaan nyata kode. Usulan
   entity baru bukan isi registry; itu keluaran `/design-business-module` setelah owner
   memutuskan.

---

## 2. Mengapa aturan ini ada

Contoh konflik yang aturan ini cegah, seluruhnya berasal dari pola yang sudah terjadi:

| Pola konflik | Kejadian nyata yang dihindari |
| --- | --- |
| Duplikasi konsep | Modul laboratorium membuat `PatientLab` sendiri, padahal pasien dimiliki `MstPatient` di Patient Management. Data pasien jadi tersebar di dua tempat dan tidak pernah cocok |
| Nama kembar | Dua area membuat entity bernama mirip, misalnya `TrxLabOrder` di Order Management dan `TrxLaboratoriumOrder` di modul lain. Developer berikutnya menebak mana yang benar |
| Rebutan kepemilikan | Dua modul sama-sama menulis ke tabel yang sama tanpa kesepakatan siapa pemiliknya. Aturan bisnis salah satu modul diam-diam dilanggar modul lain |
| Endpoint bentrok | Dua controller memakai grup Swagger `[Tags(...)]` yang sama, sehingga frontend memanggil endpoint yang salah |
| Membangun yang sudah ada | Wawancara menghasilkan keputusan membuat modul rujukan dari nol, padahal sebagian besar fondasinya sudah tersedia |

Semua pola di atas lahir dari sebab yang sama: **keputusan bisnis diambil tanpa peta sistem.**

---

## 3. Registry sistem

### 3.1 Lokasi canonical

```text
NewQuilvianSystemBackend/docs/system-registry/
├── registry-manifest.md            # identitas, SHA, kesegaran, ringkasan angka
├── 01-peta-area-dan-modul.md       # area → modul → pemilik data
├── 02-entity-terdaftar.md          # daftar entity beserta tingkat kesiapan
├── 03-kepemilikan-data-bersama.md  # data lintas modul dan siapa yang boleh menulis
├── 04-kavling-nama-dan-endpoint.md # prefix, nama terpakai, grup Swagger terdaftar
├── 05-zona-konflik.md              # temuan yang berpotensi bentrok antar modul
└── 06-indeks-entity.md             # indeks abjad seluruh entity
```

Registry ini **satu untuk seluruh sistem**, bukan satu per modul. Blueprint per modul tetap
tinggal di `docs/module-blueprints/<module>/` dan merujuk registry, bukan menyalinnya.

### 3.2 Siapa yang menulis

Hanya skill `/scan-system-registry` (pintasan `/qv-scan`). Skill lain **hanya membaca**.
Menulis registry secara manual dilarang, karena registry yang tidak dapat dihasilkan ulang
dari kode akan berbohong dalam hitungan hari.

### 3.3 Status kesegaran

| Status | Syarat | Yang boleh dilakukan |
| --- | --- | --- |
| `SEGAR` | `backend_sha` dan `frontend_sha` pada manifest sama dengan `HEAD` kedua repository | Seluruh skill boleh jalan |
| `PERLU_REFRESH` | SHA berbeda, tetapi tidak ada area atau modul baru | Jalankan `/qv-scan refresh` lebih dulu; hanya menyisir file yang berubah |
| `KADALUARSA` | Belum pernah ada scan penuh, scan penuh terakhir lebih dari 30 hari, atau ada folder area/modul baru | Jalankan `/qv-scan full` lebih dulu |

Contoh konkret: manifest mencatat `backend_sha: dd09806`. Bila hari ini `git rev-parse --short HEAD`
menghasilkan `dd09806`, registry `SEGAR` dan `/grill-me` boleh mulai. Bila menghasilkan
`a91f3c2`, registry `PERLU_REFRESH` dan agent wajib menjalankan `/qv-scan refresh` yang hanya
memeriksa file dalam `git diff --name-only dd09806..HEAD`.

Kesegaran diperiksa terhadap **kedua** repository. Registry yang segar di backend tetapi basi
di frontend tidak boleh dianggap `SEGAR`.

---

## 4. Kewajiban `/grill-me`

### 4.1 Gerbang sebelum pertanyaan pertama

Urutan yang wajib dijalankan `/grill-me`, tanpa pengecualian:

1. Baca `docs/system-registry/registry-manifest.md`.
2. Bandingkan SHA pada manifest dengan `HEAD` backend dan frontend.
3. Bila status bukan `SEGAR`, **berhenti**. Sampaikan status apa adanya dan tawarkan
   `/qv-scan refresh` atau `/qv-scan full`. Jangan mulai bertanya.
4. Bila `SEGAR`, tampilkan Kartu Konteks Pra-Wawancara pada bagian 4.2.
5. Kunci daftar **Di dalam scope** dan **Di luar scope** dengan mempertimbangkan isi Kartu
   Konteks, lalu baru ajukan pertanyaan pertama.

Registry tidak boleh dilewati dengan alasan "modulnya kecil" atau "sudah tahu isinya".
Satu-satunya jalan melewati gerbang ini adalah pengguna menyatakan secara eksplisit bahwa ia
menerima risiko konflik, dan pernyataan itu dicatat di decision log sebagai asumsi terbuka.

### 4.2 Kartu Konteks Pra-Wawancara

Bentuk wajibnya lima bagian berikut, ditulis ringkas agar muat satu layar.

```markdown
## Kartu Konteks Pra-Wawancara — modul <nama-modul>

Registry: `SEGAR` | backend `<sha>` | frontend `<sha>` | dipindai <tanggal>

### 1. Modul yang bersinggungan
| Modul | Area | Pemilik data | Titik sentuh dengan modul ini |
| --- | --- | --- | --- |
| Patient Management | HealthServices | Rekam Medis | Identitas pasien dipakai, tidak dibuat ulang |
| Registration Management | HealthServices | Pendaftaran | Encounter menjadi induk pelayanan |

### 2. Sudah tersedia dan siap dipakai ulang
| Entity | Tingkat | Pemilik | Artinya bagi wawancara ini |
| --- | --- | --- | --- |
| `MstPatient` | `L4 Terpakai` | Patient Management | Jangan tanyakan cara menyimpan identitas pasien |
| `TrxPatientEncounter` | `L4 Terpakai` | Registration Management | Jangan tanyakan cara membuat episode pelayanan |

### 3. Ada tetapi belum lengkap
| Entity | Tingkat | Yang kurang | Perlu ditanyakan? |
| --- | --- | --- | --- |
| `MstBillingItemCategory` | `L1 Terdaftar` | Belum ada controller dan migration | Ya, tanyakan apakah modul ini bergantung padanya |

### 4. Zona konflik yang menyentuh modul ini
| ID | Temuan | Risiko bila diabaikan |
| --- | --- | --- |
| KF-004 | Dua enum status pelayanan didefinisikan di dua area | Modul ini bisa memakai enum yang salah |

### 5. Pertanyaan yang tidak akan saya ajukan
Karena sudah terjawab registry:
- Apakah sistem sudah punya data pasien? Sudah, `MstPatient`, tingkat `L4`.
- Apakah sudah ada antrean? Sudah, `TrxQueue`, tingkat `L3`.
```

Bagian 5 bukan hiasan. Bagian itu memaksa agent membuktikan bahwa registry benar-benar dibaca,
dan melindungi waktu pengguna dari pertanyaan yang jawabannya sudah ada di dalam kode.

### 4.3 Larangan bertanya

`/grill-me` dilarang menanyakan hal yang sudah terjawab registry, yaitu:

- apakah suatu entity sudah ada;
- entity mana yang menyimpan suatu data;
- endpoint apa yang sudah tersedia;
- modul mana yang memiliki suatu tabel.

Yang tetap **wajib** ditanyakan kepada manusia, karena registry tidak bisa menjawabnya:

- aturan bisnis, invariant, dan batas yang sah;
- siapa yang berwenang menyetujui;
- perilaku saat gagal, dibatalkan, atau dikoreksi;
- apakah kemampuan yang sudah ada memang cocok dipakai ulang untuk kebutuhan baru.

---

## 5. Kewajiban skill lain

| Skill | Kewajiban terhadap registry |
| --- | --- |
| `/trace-existing-capabilities` | Mulai dari registry, jangan menyisir dari nol. Audit hanya memperdalam entri registry yang relevan dengan modul, lalu mengembalikan temuan baru sebagai bahan `/qv-scan refresh` |
| `/design-business-module` | Wajib memeriksa bagian kepemilikan data dan kavling nama sebelum menetapkan entity baru. Nama yang bentrok wajib diganti, bukan dipaksakan |
| `/build-module-backend` | Sebelum membuat model, controller, atau grup `[Tags(...)]` baru, periksa `04-kavling-nama-dan-endpoint.md`. Bila nama sudah terpakai, hentikan task dan laporkan |
| `/verify-module-readiness` | Boleh memakai registry sebagai pembanding, tetapi tetap membuktikan sendiri klaim kesiapan |

Registry adalah titik mulai bersama, bukan pengganti pembuktian. Klaim yang menyangkut kode
tetap wajib berbukti `repository + path + line/symbol + commit SHA`.

---

## 6. Batas kewenangan skill scan

Skill scan **hanya melaporkan fakta**. Larangan yang mengikat:

1. Dilarang mengisi kolom tindakan seperti "wajib dibuat", "harus disesuaikan", atau
   "prioritas sprint 1". Itu keputusan owner, bukan temuan mesin.
2. Dilarang mengubah source aplikasi, migration, atau konfigurasi.
3. Dilarang menyatakan sebuah entity "siap" tanpa memeriksa configuration, migration, dan
   controller-nya.
4. Dilarang menebak pemilik modul. Kepemilikan yang tidak jelas ditulis `Belum ditentukan`
   dan masuk zona konflik, bukan diisi berdasarkan perkiraan.

Pemisahan ini penting. Dokumen scan yang sudah memuat kata "wajib" membuat pembaca mengira
keputusan sudah diambil, padahal owner bisnis belum pernah dilibatkan.

---

## 7. Checklist sebelum wawancara dimulai

Periksa satu per satu:

1. `docs/system-registry/registry-manifest.md` ada.
2. Status kesegaran `SEGAR` terhadap kedua repository.
3. Kartu Konteks Pra-Wawancara sudah ditampilkan kepada pengguna.
4. Daftar **Di dalam scope** dan **Di luar scope** sudah dikunci dan dikonfirmasi.
5. Zona konflik yang menyentuh modul sudah disebutkan, bukan disembunyikan.
6. Daftar pertanyaan yang tidak akan diajukan sudah disampaikan.

Bila salah satu belum terpenuhi, wawancara belum boleh dimulai.

---

## 8. Hubungan dengan aturan lain

| Aturan | Hubungan |
| --- | --- |
| [rule-output](../rule-output/aturan-output-dokumentasi.md) | Registry adalah dokumen, sehingga tunduk pada lima aturan output: Bahasa Indonesia, mudah dipahami, detail bercontoh, proses bisnis jelas, endpoint bergaya Swagger |
| `SKILL.md` masing-masing | Aturan ini menambah gerbang masuk. Ia tidak mengubah gate approval, batas kewenangan, atau prosedur yang sudah ada |

Bila terjadi pertentangan, aturan keamanan, privasi, dan invariant pada `SKILL.md` tetap menang.
