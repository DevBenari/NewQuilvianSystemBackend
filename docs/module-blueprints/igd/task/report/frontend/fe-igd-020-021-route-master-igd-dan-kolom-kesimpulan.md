# FE-IGD-020–021 — Route Master Data IGD dan Kolom Kesimpulan Observasi

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `FE-IGD-020`, `FE-IGD-021` |
| Dasar | `BE-IGD-037` (route master data IGD berubah); temuan audit kolom layar |
| Commit dasar frontend | `a841c28b3` |
| Tanggal | 27 Agustus 2026 |
| Status | **Selesai di working tree**, belum di-commit |

---

## `FE-IGD-020` — Route master data IGD mengikuti pemindahan modul

`BE-IGD-037` memindahkan master data IGD menjadi bagian modul IGD, dan route-nya ikut berubah:

```
sebelum : /v1/health-services/master-data/emergency-installation-management/<res>
sesudah : /v1/health-services/emergency-installation-management/master-data/<res>
```

Lima pemanggilan di empat berkas disesuaikan:

| Berkas | Jumlah |
| --- | ---: |
| `lib/services/health-services/emergency-management/emergency-management-triage.service.js` | 1 |
| `lib/services/health-services/registration-management/emergency-registration.service.js` | 1 |
| `lib/services/health-services/registration-management/emergency-visit-options.service.js` | 2 |
| `lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | 1 |

Sisa rujukan route lama di `src/`: **nol**.

> **Wajib naik bersamaan.** Ini perubahan kontrak. Frontend baru terhadap backend lama akan
> `404` pada seluruh pilihan master IGD — cara kedatangan dan jenis kasus di layar pendaftaran,
> level dan indikator triase, jenis tindak lanjut di layar pengkajian. Begitu pula sebaliknya.

---

## `FE-IGD-021` — Kolom "Kesimpulan" pada Riwayat Observasi tidak pernah terisi

Tab **Observasi** pada layar pengkajian IGD mendeklarasikan kolom:

```jsx
["conclusion", "Kesimpulan"],
```

`EmergencyObservationResponse` **tidak punya** `conclusion`. Konsep yang sama ada di sana dengan
nama `completionSummary`. Akibatnya kolom itu kosong selamanya, dan karena kolom yang tidak ada
di respons tidak melempar galat, tidak ada yang menyadarinya.

Diperbaiki di frontend, bukan backend: datanya sudah ada, hanya namanya yang salah dirujuk.

```jsx
["completionSummary", "Kesimpulan"],
```

---

## Pemeriksaan menyeluruh kolom layar

Cacat di atas ditemukan lewat pemeriksaan yang membandingkan **setiap kolom yang dideklarasikan
layar pengkajian** dengan isi respons daftar yang sebenarnya. Sembilan bagian diperiksa.

| Bagian | Kolom hilang | Tindakan |
| --- | --- | --- |
| Assesmen Awal (riwayat) | `nurseNote` | Diperbaiki di backend — `BE-IGD-038` |
| Observasi | `conclusion` | Diperbaiki di frontend — `FE-IGD-021` |
| Tindakan | `procedureName`, `performedAt`, `quantity`, `performedByName` | Diperbaiki di backend — `BE-IGD-038` |
| Tanda Vital, SOAP, Catatan Terintegrasi, Tindak Lanjut, Kepergian, Resep, Nosokomial | nol | — |

Sesudah perbaikan, bagian yang punya data menunjukkan **nol kolom hilang**.

**Pelajaran:** kolom yang dideklarasikan layar tetapi tidak dikirim endpoint **tidak pernah
tampil sebagai galat**. Ia hanya kosong. Satu-satunya cara menemukannya adalah membandingkan
daftar kolom layar dengan respons sungguhan — bukan membaca kode layar, dan bukan pula membaca
DTO saja, karena keduanya terlihat wajar sendiri-sendiri.

---

## Verifikasi

```text
node --import ./tests/helpers/register.mjs --test tests/unit
=> 119 lulus, 0 gagal

npm run lint
=> 0 error, 570 warning   (seluruhnya warning gaya yang sudah ada sebelumnya)
```

Keenam endpoint master IGD dipanggil pada route baru terhadap backend yang dibangun dari
working tree: seluruhnya **200**, dengan isi seeder utuh. Route lama: **404**.

### Catatan tentang `npm test`

`npm test` menjalankan `node --test "tests/unit/**/*.test.mjs"`. Pada Node **v20** yang terpasang
di mesin ini, `node --test` **belum mendukung glob** — dukungan itu masuk pada Node 21 —
sehingga perintahnya gagal dengan `Could not find '…/tests/unit/**/*.test.mjs'` dan **tidak satu
pun test berjalan**, tetapi exit code-nya tetap `0`. Test dijalankan dengan menunjuk
direktorinya langsung.

Ini perlu diperhatikan: pada Node 20, `npm test` **tampak lulus padahal nol test berjalan**.
Perbaikannya di luar lingkup task ini — pilihannya menaikkan Node ke 21+, atau mengganti pola
glob pada `package.json` menjadi direktori.

---

## Yang belum dikerjakan

Alur simpan lewat layar sungguhan dengan kredensial petugas **masih belum pernah dijalankan**.
Bukti pada laporan ini dan `BE-IGD-036` diperoleh lewat API, memakai payload yang dibentuk
fungsi yang sama dengan yang dipakai layar (`buildPatientAssessmentPayload`) — jadi bentuk
datanya identik, tetapi jalur render dan interaksinya belum dibuktikan. Utang ini berlaku sejak
roadmap revision `1`.
